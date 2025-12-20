using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("MyTasks")]
[TaskDescription("Is the target alive?")]
public class IsTargetAlive : Conditional
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The target to check")]
    public SharedTransform target;

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null || !target.Value.gameObject.activeInHierarchy)
        {
            return TaskStatus.Failure;
        }

        return TaskStatus.Success;
    }
}
