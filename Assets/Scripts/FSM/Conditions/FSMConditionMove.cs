using UnityEngine;

[CreateAssetMenu(fileName = "FSMConditionMove", menuName = "FSM/FSMConditionMove")]
public class FSMConditionMove : FSMCondition
{
    public override bool CheckCondition(FSM controller)
    {
        return controller.GetOwner().CurrentOrder == UnitOrder.Move;
    }
}
