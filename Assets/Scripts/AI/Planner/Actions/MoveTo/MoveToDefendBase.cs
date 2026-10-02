using UnityEngine;

public class MoveToDefendBase : MoveToBase
{
    public override bool IsValid(WorldState state) => state.BaseUnderThreat && !state.InPositionAtOwnBase;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.InPositionAtOwnBase = true;
        return state;
    }

    protected override Vector3 ResolveTarget(AIContext context, Squad squad) => context.unitController.MainBase.transform.position;
}
