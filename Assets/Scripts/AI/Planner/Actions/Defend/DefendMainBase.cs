using UnityEngine;

public class DefendMainBase : DefendBase
{
    protected override bool IsInPosition(WorldState state) => state.InPositionAtOwnBase;
    protected override bool IsThreatened(WorldState state) => state.BaseUnderThreat;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.BaseUnderThreat = false;
        state.BaseAlive = true;
        return state;
    }

    public override void OnUpdate(Squad squad, AIContext context)
    {
        if (squad.MoveToTarget.HasValue) 
            return;

        squad.MoveToTarget = squad.DefenseTarget;
        squad.MoveCircleFormation(context.unitController.MainBase.transform.position);
    }
}
