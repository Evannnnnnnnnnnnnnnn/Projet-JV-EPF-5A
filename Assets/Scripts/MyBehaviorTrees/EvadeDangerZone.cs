using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using System.Reflection;

/// <summary>
/// Task de BehaviorTree qui vérifie si le drone est dans une zone de danger
/// basée sur les prédictions de roquettes ennemies.
/// Retourne Success si le drone est SAFE (pas en danger)
/// Retourne Failure si le drone est en DANGER (doit s'enfuir)
/// </summary>
[TaskCategory("MyTasks")]
[TaskDescription("Evade danger zone from enemy rocket predictions")]
public class EvadeDangerZone : Action
{
    [BehaviorDesigner.Runtime.Tasks.TooltipAttribute("Rayon de la zone de danger autour de chaque point d'impact prédit")]
    public SharedFloat dangerZoneRadius = new SharedFloat { Value = 10f };

    private PredictionManager m_PredictionManager;
    private Transform m_DroneTransform;

    public override void OnAwake()
    {
        // Récupérer le PredictionManager depuis la scène
        m_PredictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
        m_DroneTransform = transform;

        if (m_PredictionManager == null)
        {
            Debug.LogWarning("[EvadeDangerZone] PredictionManager non trouvé dans la scène!");
        }
    }

    public override TaskStatus OnUpdate()
    {
        // Si le PredictionManager n'existe pas, considérer que c'est safe
        if (m_PredictionManager == null)
        {
            return TaskStatus.Success;
        }

        // Vérifier si le drone est dans une zone de danger
        if (IsInDangerZone(m_DroneTransform.position))
        {
            Debug.Log($"[EvadeDangerZone] Drone {gameObject.name} en danger! S'enfuir!");
            return TaskStatus.Failure; // En danger -> arrête le tir et cherche une nouvelle cible
        }

        Debug.Log($"[EvadeDangerZone] Drone {gameObject.name} est safe. Peut attaquer.");
        return TaskStatus.Success; // Safe -> peut continuer
    }

    /// <summary>
    /// Vérifie si la position donnée est dans une zone de danger autour d'un impact prédit.
    /// </summary>
    private bool IsInDangerZone(Vector3 dronePosition)
    {
        // Accéder aux marqueurs d'impact du PredictionManager
        // On va utiliser la réflexion pour accéder au dictionnaire privé
        var markerField = typeof(PredictionManager).GetField("m_ImpactMarkers", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (markerField == null)
        {
            Debug.LogError("[EvadeDangerZone] Impossible d'accéder à m_ImpactMarkers!");
            return false;
        }

        var markerDict = markerField.GetValue(m_PredictionManager) as System.Collections.Generic.Dictionary<int, GameObject>;
        
        if (markerDict == null || markerDict.Count == 0)
        {
            return false; // Aucun marqueur = pas de danger
        }

        // Vérifier si le drone est à proximité de n'importe quel marqueur d'impact
        foreach (var marker in markerDict.Values)
        {
            if (marker == null) continue;

            float distance = Vector3.Distance(dronePosition, marker.transform.position);
            if (distance < dangerZoneRadius.Value)
            {
                Debug.Log($"[EvadeDangerZone] Drone trop proche du marqueur! Distance: {distance:F2}, Rayon: {dangerZoneRadius.Value}");
                return true; // En danger!
            }
        }

        return false; // Safe
    }

    // Visualiser les zones de danger dans l'éditeur (optionnel)
    private void OnDrawGizmosSelected()
    {
        if (dangerZoneRadius.Value > 0)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f); // Rouge transparent
            Gizmos.DrawWireSphere(transform.position, dangerZoneRadius.Value);
        }
    }
}
