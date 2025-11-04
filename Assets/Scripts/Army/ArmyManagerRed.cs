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
		if (m_ArmyElements.Count ==0)
		{
			GUIUtility.systemCopyBuffer = "0\t" +((int)Timer.Value).ToString()+"\t0\t0\t0";
		}
	}
	public void GreenArmyIsDead(string deadArmyTag)
 {
 int nDrones =0, nTurrets =0, health =0;
 ComputeStatistics(ref nDrones, ref nTurrets, ref health);
		GUIUtility.systemCopyBuffer = "1\t" + ((int)Timer.Value).ToString() + "\t"+nDrones.ToString()+"\t"+nTurrets.ToString()+"\t"+health.ToString();
		
		RefreshHudDisplay(); //pour une dernière mise à jour en cas de victoire
	}

}