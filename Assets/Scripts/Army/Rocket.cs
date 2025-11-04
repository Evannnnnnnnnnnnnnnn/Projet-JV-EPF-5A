using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] float m_MaxLifeDuration;
    [SerializeField] float m_DamageRadius;
    [SerializeField] float m_DamagePoints;
    [SerializeField] GameObject m_ImpactMarkerPrefab;  // Le prefab du marqueur d'impact
    [SerializeField] float m_MarkerDuration = 10f;     // Durée de vie du marqueur en secondes

    Rigidbody m_Rigidbody;
    Transform m_Transform;

    private void Awake()
	{
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Transform = transform;
        Destroy(gameObject, m_MaxLifeDuration);
    }

    private GameObject m_CurrentMarker; // Pour garder une référence au marqueur actuel

    private bool IsGreenTeam()
    {
        // Vérifier les tags possibles pour l'équipe verte
        string[] possibleGreenTags = { "Green", "TeamGreen", "GreenTeam" };
        foreach (var tag in possibleGreenTags)
        {
            if (transform.root.CompareTag(tag))
                return true;
        }
        return false;
    }

    private Vector3 PredictImpactPoint(Vector3 startPos, Vector3 endPos, float travelTime)
    {
        float sqrTravelTime = travelTime * travelTime;
        Vector3 startVelocity = (endPos - startPos - Physics.gravity * sqrTravelTime * .5f) / travelTime;

        // Simuler la trajectoire avec des petits pas de temps pour trouver l'intersection
        float timeStep = 0.1f;
        Vector3 currentPos = startPos;

        for (float t = 0; t < travelTime; t += timeStep)
        {
            Vector3 nextPos = startPos + startVelocity * t + 0.5f * Physics.gravity * t * t;

            // Vérifier s'il y a une collision entre currentPos et nextPos
            RaycastHit hit;
            Vector3 direction = (nextPos - currentPos).normalized;
            float distance = Vector3.Distance(currentPos, nextPos);

            if (Physics.Raycast(currentPos, direction, out hit, distance))
            {
                return hit.point;
            }

            currentPos = nextPos;
        }

        return endPos;
    }

    public void Shoot(Vector3 targetPos, float travelDuration)
    {
        // Calculer le point d'impact prévu
        Vector3 impactPoint = PredictImpactPoint(m_Transform.position, targetPos, travelDuration);

        // Créer un marqueur au point d'impact prévu
        if (m_ImpactMarkerPrefab != null)
        {
            m_CurrentMarker = Instantiate(m_ImpactMarkerPrefab, impactPoint, Quaternion.identity);
            // Faire en sorte que le marqueur regarde vers le haut
            m_CurrentMarker.transform.up = Vector3.up;
        }

        StartCoroutine(BallisticMoveCoroutine(travelDuration, m_Transform.position, targetPos));
        Destroy(gameObject, travelDuration + Time.fixedDeltaTime);
    }

    IEnumerator BallisticMoveCoroutine(float travelTime, Vector3 startPos, Vector3 endPos)
	{
        float sqrTravelTime = travelTime * travelTime;

        float elapsedTime = 0;
        Vector3 startVelocity = (endPos - startPos - Physics.gravity * sqrTravelTime * .5f) / travelTime;
        m_Rigidbody.MoveRotation(Quaternion.LookRotation(startVelocity.normalized));

        while (elapsedTime<travelTime)
		{
            yield return new WaitForFixedUpdate();

            // Vérifier les collisions
            RaycastHit hit;
            Vector3 currentPos = m_Rigidbody.position;
            Vector3 nextVelocity = startVelocity + Physics.gravity * (elapsedTime + Time.fixedDeltaTime);
            Vector3 nextPos = currentPos + nextVelocity * Time.fixedDeltaTime;
            
            if(Physics.Raycast(currentPos, nextVelocity.normalized, out hit, Vector3.Distance(currentPos, nextPos)))
            {
                // Si on touche quelque chose, exploser immédiatement
                if (m_CurrentMarker != null)
                {
                    Destroy(m_CurrentMarker);
                }

                // Créer l'explosion
                ExplosionManager.Instance.SpawnExplosionOnObject(hit.point, m_Transform.forward, hit.collider.gameObject, ExplosionSize.big);

                // Infliger les dégâts aux ennemis dans la zone
                Collider[] impactHitColliders = Physics.OverlapSphere(hit.point, m_DamageRadius);
                foreach (var item in impactHitColliders)
                {
                    if (!item.gameObject.CompareTag(gameObject.tag))
                        item.GetComponentInChildren<Health>()?.InflictDamage(m_DamagePoints);
                }

                Destroy(gameObject);
                yield break;
            }

            Vector3 newVelocity = startVelocity + Physics.gravity * elapsedTime;
            m_Rigidbody.AddForce(newVelocity-m_Rigidbody.linearVelocity, ForceMode.VelocityChange);
            if(m_Rigidbody.linearVelocity.sqrMagnitude>0) m_Rigidbody.MoveRotation(Quaternion.LookRotation(m_Rigidbody.linearVelocity.normalized));
            
            // Mettre à jour la position du marqueur si on en a un
            if (m_CurrentMarker != null)
            {
                Vector3 newImpactPoint = PredictImpactPoint(m_Rigidbody.position, endPos, travelTime - elapsedTime);
                m_CurrentMarker.transform.position = newImpactPoint;
            }
            
            elapsedTime += Time.fixedDeltaTime;
        }

        // Détruire le marqueur si on en a un
        if (m_CurrentMarker != null)
        {
            Destroy(m_CurrentMarker);
        }

        // Créer l'explosion
        ExplosionManager.Instance.SpawnExplosionOnObject(m_Transform.position, m_Transform.forward, TerrainManager.Instance.TerrainGO, ExplosionSize.big);

        // Infliger les dégâts aux ennemis dans la zone
        Collider[] finalHitColliders = Physics.OverlapSphere(m_Transform.position, m_DamageRadius);
        foreach (var item in finalHitColliders)
        {
            if (!item.gameObject.CompareTag(gameObject.tag))
                item.GetComponentInChildren<Health>()?.InflictDamage(m_DamagePoints);
        }

        //inflict damage to nearby enemies
        Collider[] hitColliders = Physics.OverlapSphere(endPos, m_DamageRadius);
        foreach (var item in hitColliders)
        {
            if (!item.gameObject.CompareTag(gameObject.tag))
                item.GetComponentInChildren<Health>()?.InflictDamage(m_DamagePoints);
        }

        // Détruire la rocket à la fin
        Destroy(gameObject);
    }

	//private void OnTriggerEnter(Collider other)
	//{
 //       //if (!other.CompareTag(gameObject.tag))
 //       //{
 //       //    ExplosionManager.Instance.SpawnExplosionOnObject(m_Transform.position,m_Transform.forward,other.gameObject);
 //       //    Destroy(gameObject);
 //       //}
 //   }
}
