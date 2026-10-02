using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[CreateAssetMenu(fileName = "FSMConditionAttack", menuName = "FSM/FSMConditionAttack")]
public class FSMConditionAttack : FSMCondition
{
    public override bool CheckCondition(FSM controller)
    {
        Unit owner = controller.GetOwner();

        bool condition = owner.NextTarget != null && owner.NextTarget.IsAlive && owner.CanAttack(owner.NextTarget) && owner.CurrentOrder == UnitOrder.Attack;

        if (owner.CurrentOrder == UnitOrder.Attack)
            owner.CurrentOrder = UnitOrder.None;

        
        return condition;
    }
}
