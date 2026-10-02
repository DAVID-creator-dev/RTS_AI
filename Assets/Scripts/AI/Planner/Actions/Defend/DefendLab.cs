using AI.InfluenceMap;
using UnityEngine;

public class DefendLab : DefendBase
{
    protected override bool IsInPosition(WorldState state) => state.InPositionAtOwnLab;

    protected override bool IsThreatened(WorldState state) => state.LabUnderThreat;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.LabUnderThreat = false;
        state.LabSecured = true;
        return state;
    }

    public override void OnUpdate(Squad squad, AIContext context)
    {
        if (squad.MoveToTarget.HasValue) 
            return;

        squad.MoveToTarget = squad.DefenseTarget;
        squad.MoveCircleFormation(squad.DefenseTarget);
    }
}
