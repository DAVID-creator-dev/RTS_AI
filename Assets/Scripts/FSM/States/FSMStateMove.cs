using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.GridLayoutGroup;

public class FSMStateMove : FSMState
{
    private Vector3 Target = Vector3.zero;
    Unit Owner = null;
    NavMeshAgent Agent = null;

    public override void EnterState()
    {
        Owner = GetController().GetOwner();
        Agent = Owner.GetNavMeshAgent();
        if (Agent)
        {
            Agent.SetDestination(Owner.GetPositionTarget());
            Agent.isStopped = false;
        }
    }
    public override void ExitState()
    {
        //stuff
    }
    public override FSMState UpdateState()
    {
        if (Target != Owner.GetPositionTarget())
        {
            if (Agent)
            {
                if (Owner.CurrentOrder == UnitOrder.Move)
                {
                    Owner.CurrentOrder = UnitOrder.None;
                    Agent.SetDestination(Owner.GetPositionTarget());
                    Agent.isStopped = false;
                }
                
            }
        }


        return base.UpdateState();
    }
}
