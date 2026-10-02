public class FSMStateAttack : FSMState
{
    public override void EnterState()
    {
    }
    public override void ExitState()
    {
        Unit owner = GetController().GetOwner();
        owner.CurrentOrder = UnitOrder.None;
        var agent = owner.GetNavMeshAgent();
        if (agent != null)
            agent.isStopped = false;
    }

    public override FSMState UpdateState()
    {
        GetController().GetOwner().ComputeAttack();

        return base.UpdateState();
    }
}
