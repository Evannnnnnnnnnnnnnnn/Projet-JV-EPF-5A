using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

[TaskCategory("MyTasks")]
[TaskDescription("Select non targeted enemy Drone")]

public class REDDronesSelectEnemy : Action
{
    IArmyElement m_ArmyElement;
    public SharedTransform target;
    public SharedFloat minRadius;
    public SharedFloat maxRadius;
    List<ArmyElement> _AllEnemiesTurrets;
    List<ArmyElement> _AllEnemiesDrones;
    bool _isInitialized = false;
    bool _isEven;

    private static int _nextDroneId = 0;
    private int _myDroneId;


    ArmyElement first_target;
    ArmyElement second_target;
    ArmyElement third_target;
    ArmyElement last_target;


    public override void OnAwake()
    {
        m_ArmyElement = (IArmyElement)GetComponent(typeof(IArmyElement));
        _myDroneId = System.Threading.Interlocked.Increment(ref _nextDroneId);
        _isEven = _myDroneId % 2 == 0;
    }

    public override TaskStatus OnUpdate()
    {
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

            first_target = _AllEnemiesTurrets[7];//TODO fixer à TurretGreen (1)
            second_target = _AllEnemiesTurrets[5];//TODO fixer à DroneGreen (2)
            third_target = _AllEnemiesTurrets[3];//TODO fixer à DroneRed (3)
            last_target = _AllEnemiesTurrets[6];//TODO fixer à TurretRed (8)

            _isInitialized = true;
        }

        if (target.Value == null)
        {
            _AllEnemiesTurrets.RemoveAll(item => item == null);
            _AllEnemiesDrones.RemoveAll(item => item == null);

            
            if (_myDroneId % 3 == 0 && first_target != null)
            {
                target.Value = first_target.transform;
            }
            else if (_myDroneId % 3 == 1 && second_target != null)
            {
                target.Value = second_target.transform;
            }
            else if (_myDroneId % 3 == 2 && third_target != null)
            {
                target.Value = third_target.transform;
            }
            else if (last_target != null && _myDroneId % 2 == 0)
            {
                target.Value = last_target.transform;
            }
            else if (_AllEnemiesDrones.Count > 0)
            {
                target.Value = _AllEnemiesDrones[0].transform;
            }
            else if (_AllEnemiesTurrets.Count > 0)
            {
                target.Value = _AllEnemiesTurrets[0].transform;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }

        return TaskStatus.Success;
    }
}