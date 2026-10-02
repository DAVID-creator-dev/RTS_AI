using UnityEngine;

[CreateAssetMenu(fileName = "FSMConditionMoveStop", menuName = "FSM/FSMConditionMoveStop")]
public class FSMConditionMoveStop : FSMCondition
{
    public float DistanceThreshold = 0.1f;

    public override bool CheckCondition(FSM controller)
    {
        return controller.GetOwner().HasReachedDestination(); 
    }
}
