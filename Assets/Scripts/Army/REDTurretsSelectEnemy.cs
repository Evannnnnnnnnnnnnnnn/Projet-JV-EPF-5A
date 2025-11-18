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
    bool _isInitialized = false; // Renommé pour plus de clarté
    bool _isEven;

    private static int _nextTurretId = 0;
    private int _myTurretId;


    public override void OnAwake()
    {
        m_ArmyElement = (IArmyElement)GetComponent(typeof(IArmyElement));
        _myTurretId = System.Threading.Interlocked.Increment(ref _nextTurretId);
        _isEven = (_myTurretId % 2 == 0);
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
            _isInitialized = true; // On marque comme initialisé
        }

        if (target.Value == null)
        {
            _AllEnemiesTurrets.RemoveAll(item => item == null);
            _AllEnemiesDrones.RemoveAll(item => item == null);

            // Filter by range
            var turretsInRange = _AllEnemiesTurrets.Where(item => {
                if (maxRadius.Value <= 0f) return true; // No range limit if maxRadius is 0 or less
                float dist = Vector3.Distance(transform.position, item.transform.position);
                return dist > minRadius.Value && dist < maxRadius.Value;
            }).ToList();
            
            var dronesInRange = _AllEnemiesDrones.Where(item => {
                if (maxRadius.Value <= 0f) return true; // No range limit if maxRadius is 0 or less
                float dist = Vector3.Distance(transform.position, item.transform.position);
                return dist > minRadius.Value && dist < maxRadius.Value;
            }).ToList();

            // Filter by Line of Sight (LoS)
            var turretsWithLoS = turretsInRange.Where(item => {
                Vector3 direction = (item.transform.position - transform.position).normalized;
                RaycastHit hit;
                float distance = Vector3.Distance(transform.position, item.transform.position);
                if (Physics.Raycast(transform.position, direction, out hit, distance + 0.1f)) {
                    return hit.transform == item.transform;
                }
                return false;
            }).ToList();

            var dronesWithLoS = dronesInRange.Where(item => {
                Vector3 direction = (item.transform.position - transform.position).normalized;
                RaycastHit hit;
                float distance = Vector3.Distance(transform.position, item.transform.position);
                if (Physics.Raycast(transform.position, direction, out hit, distance + 0.1f)) {
                    return hit.transform == item.transform;
                }
                return false;
            }).ToList();


                if (turretsWithLoS.Count > 0)
            {
                if (_isEven)
                {
                    target.Value = turretsWithLoS[0].transform;
                }
                else
                {
                    if (turretsWithLoS.Count > 1)
                    {
                        target.Value = turretsWithLoS[1].transform;
                    }
                    else
                    {
                        target.Value = turretsWithLoS[0].transform;
                    }
                }
            }

            else if (dronesWithLoS.Count > 0)
            {
                target.Value = dronesWithLoS[0].transform;
            }

            else
            {
                return TaskStatus.Failure;
            }
        }

        return TaskStatus.Success;
    }
}