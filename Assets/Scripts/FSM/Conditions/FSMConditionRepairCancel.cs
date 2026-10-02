using UnityEngine;

[CreateAssetMenu(fileName = "FSMConditionRepairCancel", menuName = "FSM/FSMConditionRepairCancel")]
public class FSMConditionRepairCancel : FSMCondition
{
    public override bool CheckCondition(FSM controller)
    {
        Unit owner = controller.GetOwner();
        if (owner.NextTarget == null || !owner.NextTarget.IsAlive) 
            return true;
            
        if (!owner.NextTarget.NeedsRepairing()) 
            return true;

        return !owner.CanRepair(owner.NextTarget);
    }
}
