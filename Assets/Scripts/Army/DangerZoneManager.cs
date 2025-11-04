using System.Collections.Generic;
using UnityEngine;

public class DangerZoneManager : MonoBehaviour
{
    public static DangerZoneManager Instance { get; private set; }

    [Tooltip("Rayon de danger autour des tourelles vertes")]
    public float dangerRadius = 10f;

    [Tooltip("Layer des tourelles vertes")]
    public LayerMask greenTurretLayer;

    [Tooltip("Layer des drones verts")]
    public LayerMask greenDroneLayer;

    public List<Vector3> dangerCenters = new List<Vector3>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        dangerCenters.Clear();

        // Tourelles verts (layer ou tag existant)
        Collider[] turrets = Physics.OverlapSphere(Vector3.zero, 1000f, greenTurretLayer);
        foreach (var t in turrets) dangerCenters.Add(t.transform.position);

        // Drones verts (layer des drones)
        Collider[] greenDrones = Physics.OverlapSphere(Vector3.zero, 1000f, greenDroneLayer);
        foreach (var d in greenDrones) dangerCenters.Add(d.transform.position);
    }

    void UpdateDangerZones()
    {
        dangerCenters.Clear();
        Collider[] turrets = Physics.OverlapSphere(Vector3.zero, 1000f, greenTurretLayer);

        foreach (var turret in turrets)
        {
            dangerCenters.Add(turret.transform.position);
        }
    }

    public bool IsPositionDangerous(Vector3 position)
    {
        foreach (var center in dangerCenters)
        {
            if (Vector3.Distance(center, position) < dangerRadius)
                return true;
        }
        return false;
    }

#if UNITY_EDITOR
    // Pour visualiser les zones de danger dans l'éditeur
    private void OnDrawGizmos()
    {
        if (dangerCenters == null) return;
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        foreach (var c in dangerCenters)
        {
            Gizmos.DrawSphere(c, dangerRadius);
        }
    }
#endif
}
