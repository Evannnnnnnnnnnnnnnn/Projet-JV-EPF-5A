using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDroneEvadeFromGreenProjectiles : MonoBehaviour
{
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] float safeDistance = 15f;
    [SerializeField] float checkInterval = 0.2f;

    FlyingDrone m_FlyingDrone;
    // Previously this script relied on a specific enemy projectile tag (e.g. "GreenArmyProjectile").
    // Projectiles in this project are tagged with the shooter's team tag (e.g. "GreenArmy"),
    // so we now detect any physics object that is not on our team and has a Rigidbody (i.e. a projectile).

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

            // We consider a nearby object a threatening projectile if it has a Rigidbody and
            // it is not tagged with our team tag (i.e. not friendly). This covers rockets/bullets
            // which are instantiated with the shooter's tag.
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null) continue;

            // ignore friendly projectiles
            if (go.CompareTag(gameObject.tag)) continue;

            Vector3 toDrone = transform.position - go.transform.position;
            // Use the projectile linear velocity (project uses linearVelocity elsewhere)
            var projVel = rb.linearVelocity;
            float angle = Vector3.Angle(projVel, toDrone);

            // If the projectile is generally heading towards the drone (angle < 45°)
            if (angle < 45f)
            {
                Vector3 dangerCenter = go.transform.position;
                m_FlyingDrone.FleeFrom(dangerCenter, safeDistance);
                break; // evade one projectile at a time
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
