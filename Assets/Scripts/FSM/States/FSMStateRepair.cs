using UnityEngine;

public class FSMStateRepair : FSMState
{
    public override void EnterState()
    {
    }
    public override void ExitState()
    {
        Unit owner = GetController().GetOwner();
        owner.NextTarget = null;
        owner.CurrentOrder = UnitOrder.None;
    }

    public override FSMState UpdateState()
    {
        GetController().GetOwner().ComputeRepairing();

        return base.UpdateState();
    }
}
