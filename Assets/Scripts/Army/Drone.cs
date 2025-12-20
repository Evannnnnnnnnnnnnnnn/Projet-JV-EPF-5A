using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Drone : ArmyElement,IShoot
{
    [SerializeField] GameObject m_MissilePrefab;
    [SerializeField] Transform[] m_MissileSpawnPos;
	NavMeshAgent m_NavMeshAgent;

	Transform m_Transform;

	private void Awake()
	{
		m_Transform = transform;
		m_NavMeshAgent = GetComponent<NavMeshAgent>();
	}

	public void Shoot()
	{
		//Debug.Break();
		for (int i = 0; i < m_MissileSpawnPos.Length; i++)
		{
			Transform missileSpawnPos = m_MissileSpawnPos[i];
			GameObject newMissileGO = Instantiate(m_MissilePrefab, missileSpawnPos.position, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized));
			newMissileGO.tag = gameObject.tag;
			Missile missile = newMissileGO.GetComponent<Missile>();
			missile.SetStartSpeed(m_NavMeshAgent.speed);
		}
	}

	[SerializeField] float m_DeathDamage = 10f;
	[SerializeField] float m_DeathDamageRadius = 5f;


	public bool IsKamikazeDrone = false;

	void Update()
	{
		if (IsKamikazeDrone && Input.GetKeyDown(KeyCode.K))
		{
			Die();
		}
	}

	private bool isDying = false;

	private Collider[] colliders = new Collider[32];

	new public void Die()
	{
		if (isDying) return;
		isDying = true;

		        if (gameObject.CompareTag("1"))
		        {
		            ExplosionManager.Instance.SpawnExplosionOnObject(m_Transform.position, m_Transform.forward, null, ExplosionSize.medium);
		        }		int numColliders = Physics.OverlapSphereNonAlloc(m_Transform.position, m_DeathDamageRadius, colliders);
		for (int i = 0; i < numColliders; i++)
		{
			Collider collider = colliders[i];
			if (collider.gameObject != gameObject && !collider.CompareTag(gameObject.tag))
			{
				Health health = collider.GetComponentInChildren<Health>();
				if (health != null)
				{
					health.InflictDamage(m_DeathDamage);
				}
			}
		}
		ArmyManager.ArmyElementHasBeenKilled(gameObject);
		Destroy(gameObject);
	}

}
