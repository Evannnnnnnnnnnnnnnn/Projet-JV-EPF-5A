using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using System.Linq;

[TaskCategory("MyTasks")]
[TaskDescription("Select non targeted enemy Drone")]

public class REDTurretsSelectEnemy : Action
{
    IArmyElement m_ArmyElement;
    public SharedTransform target;
    public SharedFloat minRadius;
    public SharedFloat maxRadius;
    List<ArmyElement> _AllEnemiesTurrets;
    List<ArmyElement> _AllEnemiesDrones;
    bool _isInitialized = false;
    bool _isEven;
    int firstMissiles = 6;

    ArmyElement lastTurret;

    private static int _nextTurretId = 0;
    private int _myTurretId;

    private static int _missileFiredCount = 0;

    public static void IncrementMissileCount()
    {
        _missileFiredCount++;
    }

    public static int GetMissileCount()
    {
        return _missileFiredCount;
    }


    public override void OnAwake()
    {
        m_ArmyElement = (IArmyElement)GetComponent(typeof(IArmyElement));
        _myTurretId = System.Threading.Interlocked.Increment(ref _nextTurretId);
        _isEven = _myTurretId % 2 == 0;
    }

    public override TaskStatus OnUpdate()
    {
        int nbMissile = GetMissileCount();

        // Tant que la référence n'est pas injectée par ArmyManager, on attend.
        if (m_ArmyElement.ArmyManager == null)
        {
            // On retourne "Running" pour que la tâche continue d'essayer à la frame suivante
            return TaskStatus.Running;
        }

        // On exécute cela seulement si le manager est prêt ET si on ne l'a pas déjà fait.
        if (!_isInitialized)
        {
            _AllEnemiesTurrets = m_ArmyElement.ArmyManager.GetAllEnemiesOfType<Turret>(false);
            _AllEnemiesDrones = m_ArmyElement.ArmyManager.GetAllEnemiesOfType<Drone>(false);

            lastTurret = _AllEnemiesTurrets[1];//TODO fixer à TurretGreen (6)
            
            _isInitialized = true;
        }
        else
        {
            _AllEnemiesTurrets.RemoveAll(item => item == null);
            _AllEnemiesDrones.RemoveAll(item => item == null);
        }

        if (nbMissile < firstMissiles)
        {
            GameObject targetGameObject = new GameObject("FixedTarget");
            targetGameObject.transform.position = new Vector3(9, 1, -2 + _myTurretId * 1 -5);
            target.Value = targetGameObject.transform;
        } 

        else if (_AllEnemiesTurrets.Count == 9 && nbMissile <= (9 + firstMissiles)) //normalement il faut 10 missiles, mais ça marche avec 9
        {
            GameObject targetGameObject = new GameObject("FixedTarget");
            targetGameObject.transform.position = new Vector3(-24, 3, 0);
            target.Value = targetGameObject.transform;
        }

        else if (nbMissile > (9 + firstMissiles) && nbMissile <= (19 + firstMissiles))
        {
            GameObject targetGameObject = new GameObject("FixedTarget");
            targetGameObject.transform.position = new Vector3(-22, 6, 12);
            target.Value = targetGameObject.transform;
        }

        else if (nbMissile > (19 + firstMissiles) && nbMissile <= 30 + firstMissiles)
        {
            target.Value = lastTurret.transform;
        }


        else if (_AllEnemiesDrones.Count > 0)
        {
            target.Value = _AllEnemiesDrones[0].transform;
        }

        else
        {
            return TaskStatus.Failure;
        }

        IncrementMissileCount();

        return TaskStatus.Success;
    }
}