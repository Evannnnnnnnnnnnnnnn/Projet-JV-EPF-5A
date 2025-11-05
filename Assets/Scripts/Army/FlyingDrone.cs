using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlyingDrone : ArmyElement,IShoot
{
    [SerializeField] GameObject m_BulletPrefab;
    [SerializeField] Transform[] m_BulletSpawnPos;
	[SerializeField] ParticleSystem[] m_ParticleSystems;

	[SerializeField] float m_MoveSpeed =10f;

	Transform m_Transform;

	Coroutine m_MoveCoroutine = null;

	private void Awake()
	{
		m_Transform = transform;
	}

	public void Shoot()
	{
		//Debug.Break();
		for (int i =0; i < m_BulletSpawnPos.Length; i++)
		{
			Transform bulletSpawnPos = m_BulletSpawnPos[i];
			GameObject newBulletGO = Instantiate(m_BulletPrefab, bulletSpawnPos.position, Quaternion.LookRotation(bulletSpawnPos.forward,Vector3.up));
			newBulletGO.tag = gameObject.tag;
		}
	}

<<<<<<< Updated upstream
	public void Die()
=======
	// Aim precisely at a world-space target position when shooting.
	public void ShootAt(Vector3 targetWorldPosition)
	{
		for (int i = 0; i < m_BulletSpawnPos.Length; i++)
		{
			Transform bulletSpawnPos = m_BulletSpawnPos[i];
			Vector3 aimDir = (targetWorldPosition - bulletSpawnPos.position).normalized;
			if (aimDir.sqrMagnitude < 0.0001f) aimDir = bulletSpawnPos.forward;
			GameObject newBulletGO = Instantiate(m_BulletPrefab, bulletSpawnPos.position, Quaternion.LookRotation(aimDir, Vector3.up));
			newBulletGO.tag = gameObject.tag;
		}
	}

	new public void Die()
>>>>>>> Stashed changes
	{
		ArmyManager.ArmyElementHasBeenKilled(gameObject);
		Destroy(gameObject);
	}

	// Public API to command movement for flying drones
	public void CommandMoveTo(Vector3 destination, float speed = -1f)
	{
		if (m_MoveCoroutine != null) StopCoroutine(m_MoveCoroutine);
		float useSpeed = speed >0 ? speed : m_MoveSpeed;
		m_MoveCoroutine = StartCoroutine(MoveToCoroutine(destination, useSpeed));
	}

	public void StopCommand()
	{
		if (m_MoveCoroutine != null)
		{
			StopCoroutine(m_MoveCoroutine);
			m_MoveCoroutine = null;
		}
	}

	public void FleeFrom(Vector3 dangerCenter, float safeDistance, float speed = -1f)
	{
		Vector3 pos = transform.position;
		Vector3 dir = pos - dangerCenter;
		if (dir.sqrMagnitude <0.0001f)
		{
			// fallback direction if coincident
			var cam = Camera.main;
			dir = cam ? (pos - cam.transform.position).normalized : Vector3.back;
		}
		else dir.Normalize();

		Vector3 destination = pos + dir * safeDistance;
		CommandMoveTo(destination, speed);
	}

	IEnumerator MoveToCoroutine(Vector3 destination, float speed)
	{
		while ((transform.position - destination).sqrMagnitude >0.01f)
		{
			transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
			yield return null;
		}
		m_MoveCoroutine = null;
	}

}
