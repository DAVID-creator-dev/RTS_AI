using UnityEngine;

[CreateAssetMenu(fileName = "FSMConditionAttackCancel", menuName = "FSM/FSMConditionAttackCancel")]
public class FSMConditionAttackCancel : FSMCondition
{
    public override bool CheckCondition(FSM controller)
    {
        return controller.GetOwner().NextTarget == null || controller.GetOwner().NextTarget.IsAlive == false || controller.GetOwner().CanAttack(controller.GetOwner().NextTarget) == false;
    }
}
