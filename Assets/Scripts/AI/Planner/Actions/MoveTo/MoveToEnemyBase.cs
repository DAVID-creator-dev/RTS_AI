using UnityEngine;

public class MoveToEnemyBase : MoveToBase
{
    public override bool IsValid(WorldState state) => state.EnemyBaseLocated && !state.InPositionAtEnemyBase;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.InPositionAtEnemyBase = true;
        return state;
    }

    protected override Vector3 ResolveTarget(AIContext context, Squad squad)
    {
        ETeam opponent = GameServices.GetOpponent(context.unitController.GetTeam());
        return GameServices.GetControllerByTeam(opponent).MainBase.transform.position;
    }
}
