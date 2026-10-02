using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[CreateAssetMenu(fileName = "FSMConditionCaptureCancel", menuName = "FSM/FSMConditionCaptureCancel")]
public class FSMConditionCaptureCancel : FSMCondition
{
    public override bool CheckCondition(FSM controller)
    {
        Unit Owner = controller.GetOwner();
        if (Owner.CaptureTarget == null)
            return true;

        return Owner.GetTeam() == Owner.CaptureTarget.GetTeam();
    }
}
