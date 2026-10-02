using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

[CreateAssetMenu(fileName = "FSMConditionRepair", menuName = "FSM/FSMConditionRepair")]
public class FSMConditionRepair : FSMCondition
{
    public override bool CheckCondition(FSM controller)
    {

        Unit owner = controller.GetOwner();
        if (owner.NextTarget == null)
            return false;


        bool condition = owner.CurrentOrder == UnitOrder.Repair &&
           owner.CanRepair(owner.NextTarget) &&
           owner.GetTeam() == owner.NextTarget.GetTeam() && owner.NextTarget.IsAlive && owner.GetUnitData.CanRepair; 

        if (owner.CurrentOrder == UnitOrder.Repair)
            owner.CurrentOrder = UnitOrder.None;
        return condition;
    }
}
