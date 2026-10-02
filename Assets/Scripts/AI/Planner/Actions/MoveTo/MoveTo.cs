using UnityEngine;

public abstract class MoveToBase : Action
{
    protected abstract Vector3 ResolveTarget(AIContext context, Squad squad);

    public override void OnUpdate(Squad squad, AIContext context)
    {        
        if (squad.MoveToTarget.HasValue)
            return;

        squad.MoveToTarget = ResolveTarget(context, squad);
        squad.MoveFormation(squad.MoveToTarget.Value);
    }

    public override bool Complete(Squad squad, AIContext context)
    {
        if (!squad.HasReachedPos())
            return false;

        squad.MoveToTarget = null;
        return true;
    }
}
