using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "FSMConditionCapture", menuName = "FSM/FSMConditionCapture")]
public class FSMConditionCapture : FSMCondition
{
    public override bool CheckCondition(FSM controller)
    {
        Unit Owner = controller.GetOwner();

        if (Owner.CaptureTarget == null)
            return false;
            
        return Owner.CanCapture(Owner.CaptureTarget) && Owner.CurrentOrder == UnitOrder.Capture && Owner.GetTeam() != Owner.CaptureTarget.GetTeam();
    }
}
