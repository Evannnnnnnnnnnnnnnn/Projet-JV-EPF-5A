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

    bool _isInitialized = false; // Renommé pour plus de clarté

	public override void OnAwake()
	{
        m_ArmyElement = (IArmyElement) GetComponent(typeof(IArmyElement));
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
            _AllEnemiesTurrets = m_ArmyElement.ArmyManager.GetAllEnemiesOfType<Turret>(true);
            _AllEnemiesDrones = m_ArmyElement.ArmyManager.GetAllEnemiesOfType<Drone>(true);

            _isInitialized = true; // On marque comme initialisé
        }

        if (target.Value == null)
        {
            _AllEnemiesTurrets.RemoveAll(item => item == null);
            _AllEnemiesDrones.RemoveAll(item => item == null);


            if (_AllEnemiesTurrets.Count > 0)
            {
                target.Value = _AllEnemiesTurrets[0].transform;
            }
            else if (_AllEnemiesDrones.Count > 0)
            {
                target.Value = _AllEnemiesDrones[0].transform;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }

        return TaskStatus.Success;
	}
}