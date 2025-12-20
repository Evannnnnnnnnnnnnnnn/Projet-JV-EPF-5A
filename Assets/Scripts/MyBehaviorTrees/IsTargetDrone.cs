using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("MyTasks")]
[TaskDescription("Is the target a drone?")]
public class IsTargetDrone : Conditional
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The target to check")]
    public SharedTransform target;

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null)
        {
            return TaskStatus.Failure;
        }

        if (target.Value.GetComponent<Drone>() != null)
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }
}
