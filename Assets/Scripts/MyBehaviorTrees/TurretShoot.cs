using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("MyTasks")]
[TaskDescription("Turret shoots a rocket towards target")]

public class TurretShoot: Action
{
	public SharedTransform target;
	Turret turret;

	public override void OnAwake()
	{
		turret = GetComponent<Turret>();
	}

	public override TaskStatus OnUpdate()
	{
		if (turret!= null && target.Value != null)
		{
			// Check line of sight from a spawn point before shooting. If blocked, return Failure so the AI can choose another action.
			Transform[] spawns = turret.m_SpawnPoints;
			Transform spawn = (spawns != null && spawns.Length > 0) ? spawns[0] : turret.transform;
			Vector3 dir = (target.Value.position - spawn.position).normalized;
			float dist = Vector3.Distance(spawn.position, target.Value.position);
			if (Physics.Raycast(spawn.position, dir, out RaycastHit hit, dist))
			{
				if (hit.transform != target.Value)
				{
					return TaskStatus.Failure; // blocked
				}
			}
			// clear -> shoot
			turret.Shoot(target.Value.position);
			return TaskStatus.Success;
		}
		else return TaskStatus.Failure;
	}


}

//QUARANTINE
/*
 * turret.ArmyManager.UnlockArmyElement(gameObject);
 */