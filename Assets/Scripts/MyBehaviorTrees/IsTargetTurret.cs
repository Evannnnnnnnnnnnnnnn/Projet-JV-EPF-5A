using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("MyTasks")]
[TaskDescription("Is the target a turret?")]
public class IsTargetTurret : Conditional
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The target to check")]
    public SharedTransform target;

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null)
        {
            return TaskStatus.Failure;
        }

        if (target.Value.GetComponent<Turret>() != null)
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }
}
