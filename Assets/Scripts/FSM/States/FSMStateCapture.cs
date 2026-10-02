using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class FSMStateCapture : FSMState
{
    public override void EnterState()
    {
        Unit Owner = GetController().GetOwner();
        TargetBuilding CaptureTarget = Owner.CaptureTarget;

        if (Owner.GetNavMeshAgent())
            Owner.GetNavMeshAgent().isStopped = true;

        CaptureTarget.StartCapture(Owner);
    }
    public override void ExitState()
    {
        Unit Owner = GetController().GetOwner();
        TargetBuilding CaptureTarget = Owner.CaptureTarget;

        if (CaptureTarget != null)
        {
            CaptureTarget.StopCapture(Owner);
            Owner.CaptureTarget = null;
        }

        GetController().GetOwner().CurrentOrder = UnitOrder.None;
    }

    public override FSMState UpdateState()
    {
        return base.UpdateState();
    }
}
