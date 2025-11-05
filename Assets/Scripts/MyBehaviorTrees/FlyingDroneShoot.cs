using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("MyTasks")]
[TaskDescription("Flying Drone Shoots")]

public class FlyingDroneShoot : Action
{
	[SerializeField] float m_ShootingPeriod;
	[SerializeField] float m_SidestepDistance = 5f;
	float m_Timer;

	public SharedTransform target; // optional: behavior tree can provide a target

	FlyingDrone drone;

	public override void OnStart()
	{
		drone = GetComponent<FlyingDrone>();
	}

	public override TaskStatus OnUpdate()
	{
		m_Timer -= Time.deltaTime;
		if (m_Timer < 0)
		{
			// If we have a target, check line-of-sight before shooting
			if (target != null && target.Value != null)
			{
				Vector3 from = transform.position;
				Vector3 to = target.Value.position;
				Vector3 dir = (to - from).normalized;
				float dist = Vector3.Distance(from, to);

				if (Physics.Raycast(from, dir, out RaycastHit hit, dist))
				{
					// If something blocks the LOS and it's not the target, sidestep instead of shooting
					if (hit.transform != target.Value)
					{
						// try to find a safe sidestep position that clears the LOS
						Vector3 lateral = Vector3.Cross(dir, Vector3.up).normalized;
						lateral *= (Random.value > 0.5f) ? 1f : -1f;
						Vector3 newPos;
						if (FindSafeSidestep(transform.position, target.Value.position, dir, lateral, m_SidestepDistance, out newPos))
						{
							drone?.CommandMoveTo(newPos);
							m_Timer = m_ShootingPeriod;
							return TaskStatus.Success;
						}
						else
						{
							// fallback: do a small lateral move anyway
							newPos = transform.position + lateral * (m_SidestepDistance * 0.5f);
							drone?.CommandMoveTo(newPos);
							m_Timer = m_ShootingPeriod;
							return TaskStatus.Success;
						}
					}
				}
				// clear LOS -> shoot precisely at the target
				drone?.ShootAt(target.Value.position);
			}
			else
			{
				// no explicit target: fallback to regular shooting
				drone?.Shoot();
			}

			m_Timer = m_ShootingPeriod;
		}

		return TaskStatus.Running;
	}

	public override void OnReset()
	{
		base.OnReset();
		m_Timer = m_ShootingPeriod;
	}

	// Try multiple sidestep candidates and ensure the new position has a clear LOS to the target
	bool FindSafeSidestep(Vector3 currentPos, Vector3 targetPos, Vector3 forwardDir, Vector3 lateralDir, float sidestepDistance, out Vector3 result)
	{
		// sample distances and small elevation offsets
		float[] distances = new float[] { sidestepDistance, sidestepDistance * 1.5f, sidestepDistance * 2f };
		float[] heights = new float[] { 0f, 1f, -1f };

		float clearanceRadius = 0.6f; // radius to check for collisions

		foreach (var d in distances)
		{
			foreach (var h in heights)
			{
				Vector3 candidate = currentPos + lateralDir * d + Vector3.up * h;

				// don't pick a candidate inside a collider
				Collider[] coll = Physics.OverlapSphere(candidate, clearanceRadius);
				bool blocked = false;
				foreach (var c in coll)
				{
					// ignore triggers and the drone itself
					if (c.gameObject == this.gameObject) continue;
					if (!c.isTrigger) { blocked = true; break; }
				}
				if (blocked) continue;

				// check LOS from candidate to target
				Vector3 dir = (targetPos - candidate).normalized;
				float dist = Vector3.Distance(candidate, targetPos);
				if (Physics.Raycast(candidate, dir, out RaycastHit hit, dist))
				{
					if (hit.transform == target.Value) // clear
					{
						result = candidate;
						return true;
					}
					else continue; // blocked
				}
				else
				{
					// nothing hit -> clear LOS
					result = candidate;
					return true;
				}
			}
		}

		result = Vector3.zero;
		return false;
	}
}
