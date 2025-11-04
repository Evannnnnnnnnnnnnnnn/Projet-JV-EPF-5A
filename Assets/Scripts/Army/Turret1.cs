using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDroneEvadeFromGreenProjectiles : MonoBehaviour
{
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] float safeDistance = 15f;
    [SerializeField] float checkInterval = 0.2f;

    FlyingDrone m_FlyingDrone;
    string enemyTag = "GreenArmy"; // à adapter selon le tag des drones verts

    void Awake()
    {
        m_FlyingDrone = GetComponent<FlyingDrone>();
    }

    void OnEnable()
    {
        StartCoroutine(CheckProjectilesRoutine());
    }

    IEnumerator CheckProjectilesRoutine()
    {
        while (true)
        {
            DetectAndEvade();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void DetectAndEvade()
    {
        // Récupère tous les projectiles dans un rayon donné
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            GameObject go = hit.gameObject;

            // On ne s'intéresse qu'aux projectiles ennemis
            if (go.CompareTag(enemyTag + "Projectile")) // Ex: "GreenArmyProjectile"
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb == null) continue;

                Vector3 toDrone = transform.position - go.transform.position;
                float angle = Vector3.Angle(rb.linearVelocity, toDrone);

                // Si le projectile se dirige vers le drone (moins de 45° de différence)
                if (angle < 45f)
                {
                    Vector3 dangerCenter = go.transform.position;
                    m_FlyingDrone.FleeFrom(dangerCenter, safeDistance);
                    break; // on esquive un projectile à la fois
                }
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}
