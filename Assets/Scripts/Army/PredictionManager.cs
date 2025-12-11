using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ce manager gère les prédictions de trajectoires pour les projectiles.
/// </summary>
public class PredictionManager : MonoBehaviour
{
    [Tooltip("Le prefab du marqueur d'impact à utiliser.")]
    [SerializeField] private GameObject m_ImpactMarkerPrefab;

    [Tooltip("Le tag utilisé pour identifier les roquettes de l'équipe verte. Les roquettes héritent du tag de leur tourelle.")]
    [SerializeField] private string m_GreenTeamTag = "2";

    [Tooltip("La hauteur du sol. Mettez à jour si votre terrain n'est pas à y=0.")]
    [SerializeField] private float m_GroundHeight = 0f;

    // Dictionnaire pour associer un marqueur à chaque roquette via son ID unique
    private Dictionary<int, GameObject> m_ImpactMarkers = new Dictionary<int, GameObject>();
    private Dictionary<int, RocketTrajectoryData> m_RocketTrajectories = new Dictionary<int, RocketTrajectoryData>();
    private List<Rocket> m_ActiveRockets = new List<Rocket>();
    private List<int> m_MarkersToRemove = new List<int>();

    // Structure pour stocker les données de trajectoire des roquettes
    private struct RocketTrajectoryData
    {
        public Vector3 PreviousPosition;
        public Vector3 PreviousVelocity;
        public float LastDetectionTime;
        public bool HasPreviousFrame;
    }

    // --- DEBUG ---
    private float m_DebugTimer = 0f;
    private const float DEBUG_LOG_INTERVAL = 1.0f; // N'affiche les logs qu'une fois par seconde

    void Awake()
    {
        // Détecter automatiquement la hauteur du terrain au démarrage
        if (Physics.Raycast(new Vector3(0, 1000f, 0), Vector3.down, out RaycastHit hit, 2000f))
        {
            m_GroundHeight = hit.point.y;
            Debug.Log($"[PredictionManager] Hauteur du terrain détectée automatiquement : {m_GroundHeight}");
        }
        else
        {
            Debug.LogWarning("[PredictionManager] Impossible de détecter la hauteur du terrain. Utilisation de la valeur par défaut.");
        }
    }

    void Update()
    {
        // --- DEBUG: Vérifier si le manager est actif ---
        m_DebugTimer += Time.deltaTime;
        if (m_DebugTimer > DEBUG_LOG_INTERVAL)
        {
            m_DebugTimer = 0f;
            if (m_ImpactMarkerPrefab == null)
            {
                Debug.LogWarning("[PredictionManager] Le prefab du marqueur d'impact n'est PAS ASSIGNÉ dans l'inspecteur !");
            }
        }

        // 1. Trouver toutes les roquettes actives de l'équipe verte
        m_ActiveRockets.Clear();
        Rocket[] allRockets = FindObjectsOfType<Rocket>();
        
        foreach (Rocket rocket in allRockets)
        {
            // Vérifier que la roquette appartient à l'équipe verte (tag "2") et a un Rigidbody
            if (rocket.gameObject.CompareTag(m_GreenTeamTag) && rocket.m_Rigidbody != null)
            {
                m_ActiveRockets.Add(rocket);
            }
        }

        // --- DEBUG: Vérifier si des roquettes sont trouvées ---
        if (m_DebugTimer == 0f && m_ActiveRockets.Count == 0)
        {
            Debug.Log("[PredictionManager] Aucune roquette avec le tag '" + m_GreenTeamTag + "' n'a été trouvée.");
        }

        // 2. Mettre à jour ou créer les marqueurs pour chaque roquette en vol 
        HashSet<int> activeRocketIds = new HashSet<int>();
        foreach (Rocket rocket in m_ActiveRockets)
        {
           int rocketId = rocket.GetInstanceID();
           activeRocketIds.Add(rocketId);

            // Mettre à jour les données de trajectoire
            UpdateRocketTrajectory(rocket, rocketId);

            // Calculer le point d'impact basé sur la trajectoire observée
            Vector3 impactPoint = PredictImpactPoint(rocket, rocketId);

            if(m_ImpactMarkers.ContainsKey(rocketId))
            {
                // Mettre à jour la position du marqueur existant
                m_ImpactMarkers[rocketId].transform.position = impactPoint;
            }
            else
            {
                // Créer un nouveau marqueur uniquement si on a au moins 2 frames de données
                if (m_RocketTrajectories.ContainsKey(rocketId) && m_RocketTrajectories[rocketId].HasPreviousFrame)
                {
                    Debug.Log($"[PredictionManager] Nouvelle roquette détectée (ID: {rocketId}). Prédiction du point d'impact à : {impactPoint}");

                    if (m_ImpactMarkerPrefab != null)
                    {
                        GameObject marker = Instantiate(m_ImpactMarkerPrefab, impactPoint, Quaternion.identity);
                        m_ImpactMarkers.Add(rocketId, marker);
                        Debug.Log($"[PredictionManager] MARQUEUR CRÉÉ pour la roquette {rocketId}.");
                    }
                }
            }
        }

        // 3. Supprimer les marqueurs pour les roquettes qui ne sont plus actives
        m_MarkersToRemove.Clear();
        foreach (var entry in m_ImpactMarkers)
        {
            if (!activeRocketIds.Contains(entry.Key))
            {
                Debug.Log($"[PredictionManager] Suppression du marqueur pour l'ancienne roquette (ID: {entry.Key}).");
                Destroy(entry.Value);
                m_MarkersToRemove.Add(entry.Key);
            }
        }
        foreach (int id in m_MarkersToRemove)
        {
            m_ImpactMarkers.Remove(id);
            m_RocketTrajectories.Remove(id); // Nettoyer aussi les données de trajectoire
        }
    }   

    /// <summary>
    /// Met à jour les données de trajectoire d'une roquette en enregistrant sa position et vélocité actuelle.
    /// </summary>
    private void UpdateRocketTrajectory(Rocket rocket, int rocketId)
    {
        Vector3 currentPos = rocket.transform.position;
        Vector3 currentVelocity = rocket.m_Rigidbody.linearVelocity;

        if (!m_RocketTrajectories.ContainsKey(rocketId))
        {
            // Première détection : enregistrer la position et vélocité
            RocketTrajectoryData data = new RocketTrajectoryData
            {
                PreviousPosition = currentPos,
                PreviousVelocity = currentVelocity,
                LastDetectionTime = Time.time,
                HasPreviousFrame = false
            };
            m_RocketTrajectories[rocketId] = data;
        }
        else
        {
            // Mise à jour : la nouvelle position devient la précédente
            RocketTrajectoryData data = m_RocketTrajectories[rocketId];
            data.PreviousPosition = currentPos;
            data.PreviousVelocity = currentVelocity;
            data.LastDetectionTime = Time.time;
            data.HasPreviousFrame = true; // Nous avons maintenant 2 frames !
            m_RocketTrajectories[rocketId] = data;
        }
    }

    /// <summary>
    /// Prédit le point d'impact en se basant sur une estimation itérative pour plus de précision sur terrain varié.
    /// </summary>
    private Vector3 PredictImpactPoint(Rocket rocket, int rocketId)
    {
        Vector3 currentPos = rocket.transform.position;
        Vector3 currentVelocity = rocket.m_Rigidbody.linearVelocity;
        float gravity = Physics.gravity.y;

        // --- Première estimation basée sur une hauteur de sol moyenne ---
        float estimatedTimeToImpact = CalculateImpactTime(currentPos.y, currentVelocity.y, gravity, m_GroundHeight);
        if (estimatedTimeToImpact < 0)
        {
            // La roquette ne semble pas se diriger vers le sol, utiliser sa position actuelle
            return new Vector3(currentPos.x, m_GroundHeight, currentPos.z);
        }

        // Estimer la position horizontale (X, Z) de l'impact
        float estimatedImpactX = currentPos.x + currentVelocity.x * estimatedTimeToImpact;
        float estimatedImpactZ = currentPos.z + currentVelocity.z * estimatedTimeToImpact;

        // --- Deuxième étape : affiner la hauteur du sol ---
        // Lancer un rayon depuis le ciel à la position estimée pour trouver la vraie hauteur du sol
        float actualGroundHeight = m_GroundHeight;
        // On part de très haut pour être sûr d'être au-dessus du terrain
        if (Physics.Raycast(new Vector3(estimatedImpactX, currentPos.y + 1000f, estimatedImpactZ), Vector3.down, out RaycastHit hit, 2000f))
        {
            actualGroundHeight = hit.point.y;
        }

        // --- Troisième étape : recalculer le temps d'impact avec la hauteur de sol précise ---
        float finalTimeToImpact = CalculateImpactTime(currentPos.y, currentVelocity.y, gravity, actualGroundHeight);
        if (finalTimeToImpact < 0)
        {
             // Si le nouveau calcul échoue, on garde la première estimation qui est plus robuste
            finalTimeToImpact = estimatedTimeToImpact;
        }

        // Calculer le point d'impact final (X, Z) avec le temps affiné
        float finalImpactX = currentPos.x + currentVelocity.x * finalTimeToImpact;
        float finalImpactZ = currentPos.z + currentVelocity.z * finalTimeToImpact;
        
        // Le point d'impact final se trouve à la hauteur réelle du sol
        return new Vector3(finalImpactX, actualGroundHeight, finalImpactZ);
    }

    /// <summary>
    /// Calcule le temps nécessaire pour qu'un projectile atteigne une certaine altitude en chute libre.
    /// </summary>
    /// <param name="y0">Position verticale initiale.</param>
    /// <param name="v0y">Vitesse verticale initiale.</param>
    /// <param name="g">Gravité (doit être une valeur négative).</param>
    /// <param name="targetY">Altitude cible.</param>
    /// <returns>Le temps d'impact en secondes (> 0), ou -1 si la cible est inatteignable.</returns>
    private float CalculateImpactTime(float y0, float v0y, float g, float targetY)
    {
        // Résolution de l'équation quadratique pour le temps t :
        // targetY = y0 + v0y*t + 0.5*g*t^2
        // Réarrangé : (0.5*g)*t^2 + (v0y)*t + (y0 - targetY) = 0
        float a = 0.5f * g;
        float b = v0y;
        float c = y0 - targetY;

        // Le discriminant nous dit s'il y a des solutions réelles
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            // Pas de solution réelle, le projectile n'atteindra jamais cette altitude.
            return -1f;
        }

        // Deux solutions possibles pour le temps
        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDiscriminant) / (2 * a);
        float t2 = (-b - sqrtDiscriminant) / (2 * a);

        // On veut la plus petite solution positive, car c'est le premier impact dans le futur.
        if (t1 > 0 && t2 > 0)
        {
            return Mathf.Min(t1, t2);
        }
        else if (t1 > 0)
        {
            return t1;
        }
        else if (t2 > 0)
        {
            return t2;
        }
        
        // Aucune solution dans le futur.
        return -1f;
    }
}