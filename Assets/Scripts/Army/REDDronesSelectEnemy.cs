using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

[TaskCategory("MyTasks")]
[TaskDescription("Select enemy turret first, then drone. Keep target until destroyed.")]

public class REDDronesSelectEnemy : Action
{
    IArmyElement m_ArmyElement;
    public SharedTransform target;
    public SharedFloat minRadius;
    public SharedFloat maxRadius;
    
    // Cache des ennemis (mis à jour occasionnellement)
    private List<ArmyElement> m_CachedEnemiesTurrets = new List<ArmyElement>();
    private List<ArmyElement> m_CachedEnemiesDrones = new List<ArmyElement>();
    private float m_LastCacheUpdateTime = -999f;
    private const float CACHE_UPDATE_INTERVAL = 1f; // Mettre à jour le cache chaque seconde

    public override void OnAwake()
    {
        m_ArmyElement = (IArmyElement)GetComponent(typeof(IArmyElement));
        // _myDroneId = System.Threading.Interlocked.Increment(ref _nextDroneId);
        // _isEven = _myDroneId % 2 == 0;
    }

    public override TaskStatus OnUpdate()
    {
        // Tant que le ArmyManager n'est pas prêt, on attend
        if (m_ArmyElement.ArmyManager == null)
        {
            return TaskStatus.Running;
        }

        // Si on a déjà une cible valide, la garder
        if (target.Value != null)
        {
            return TaskStatus.Success;
        }

        // Mettre à jour le cache occasionnellement
        if (Time.time - m_LastCacheUpdateTime > CACHE_UPDATE_INTERVAL)
        {
            m_CachedEnemiesTurrets = m_ArmyElement.ArmyManager.GetAllEnemiesOfType<Turret>(false);
            m_CachedEnemiesDrones = m_ArmyElement.ArmyManager.GetAllEnemiesOfType<Drone>(false);
            m_LastCacheUpdateTime = Time.time;
        }

        Vector3 dronePosition = transform.position;

        // 1. Chercher les TurretGreen (tag "2") dans le rayon
        ArmyElement closestTurret = FindClosestEnemy(m_CachedEnemiesTurrets, dronePosition, "2");
        if (closestTurret != null)
        {
            target.Value = closestTurret.transform;
            Debug.Log($"[REDDronesSelectEnemy] ✓ {gameObject.name}: Cible TurretGreen = {target.Value.name}");
            return TaskStatus.Success;
        }

        // 2. Si pas de turret, chercher les DroneGreen (tag "2") dans le rayon
        ArmyElement closestDrone = FindClosestEnemy(m_CachedEnemiesDrones, dronePosition, "2");
        if (closestDrone != null)
        {
            target.Value = closestDrone.transform;
            Debug.Log($"[REDDronesSelectEnemy] ✓ {gameObject.name}: Cible DroneGreen = {target.Value.name}");
            return TaskStatus.Success;
        }

        // 3. Aucune cible trouvée - retourner Running au lieu de Failure
        Debug.Log($"[REDDronesSelectEnemy] {gameObject.name}: Pas de cible dans le rayon");
        return TaskStatus.Running;
    }

    /// <summary>
    /// Trouve l'ennemi le plus proche dans le rayon spécifié
    /// </summary>
    private ArmyElement FindClosestEnemy(List<ArmyElement> enemies, Vector3 dronePosition, string requiredTag)
    {
        ArmyElement closest = null;
        float closestDistance = float.MaxValue;

        foreach (ArmyElement enemy in enemies)
        {
            // Vérifier que l'ennemi existe et a le bon tag
            if (enemy == null || !enemy.gameObject.CompareTag(requiredTag))
                continue;

            // Vérifier la distance
            float distance = Vector3.Distance(dronePosition, enemy.transform.position);
            if (distance < minRadius.Value || distance > maxRadius.Value)
                continue;

            // Garder le plus proche
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }
}