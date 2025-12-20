using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.AI;

public class ArmyManagerRed : ArmyManager
{
	[SerializeField] float flyingSafeDistanceMultiplier =2f;

	public void CommandFlyingDronesAvoidDanger()
	{
		if (DangerZoneManager.Instance == null) return;

		// Iterate over a snapshot of army elements
		foreach (var elem in m_ArmyElements.ToList())
		{
			if (elem is FlyingDrone fd)
			{
				Vector3 pos = fd.transform.position;
				Vector3 nearest = GetNearestDangerCenter(pos);
				float safeDist = DangerZoneManager.Instance.dangerRadius * flyingSafeDistanceMultiplier;
				fd.FleeFrom(nearest, safeDist);
			}
		}
	}

	Vector3 GetNearestDangerCenter(Vector3 pos)
	{
		var dz = DangerZoneManager.Instance;
		if (dz == null || dz.dangerCenters == null || dz.dangerCenters.Count ==0) return pos;

		Vector3 best = dz.dangerCenters[0];
		float bestDist = Vector3.Distance(pos, best);
		for (int i =1; i < dz.dangerCenters.Count; i++)
		{
			float d = Vector3.Distance(pos, dz.dangerCenters[i]);
			if (d < bestDist)
			{
				bestDist = d;
				best = dz.dangerCenters[i];
			}
		}
		return best;
	}

	public override void ArmyElementHasBeenKilled(GameObject go)
	{
		base.ArmyElementHasBeenKilled(go);
		if (m_ArmyElements.Count == 0)
		{
			ArmyManagerGreen greenManager = Object.FindObjectOfType<ArmyManagerGreen>();
			if (greenManager != null)
			{
				int nDrones = 0, nTurrets = 0, health = 0;
				greenManager.GetStatistics(ref nDrones, ref nTurrets, ref health);
				BattleDataExporter.WriteBattleData(SimulationManager.CurrentBattleIndex, false, Timer.Value, nDrones, nTurrets, health);
			}
			else
			{
				BattleDataExporter.WriteBattleData(SimulationManager.CurrentBattleIndex, false, Timer.Value, 0, 0, 0);
			}
			SimulationManager.NotifyBattleEnded();
		}
	}
	public void GreenArmyIsDead(string deadArmyTag)
	{
		int nDrones = 0, nTurrets = 0, health = 0;
		ComputeStatistics(ref nDrones, ref nTurrets, ref health);
		BattleDataExporter.WriteBattleData(SimulationManager.CurrentBattleIndex, true, Timer.Value, nDrones, nTurrets, health);
		SimulationManager.NotifyBattleEnded();
		RefreshHudDisplay(); //pour une dernire mise  jour en cas de victoire
	}

}