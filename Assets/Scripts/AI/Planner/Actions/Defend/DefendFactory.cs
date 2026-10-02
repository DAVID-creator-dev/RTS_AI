public class DefendFactory : DefendBase
{
    protected override bool IsInPosition(WorldState state) => state.InPositionAtOwnFactory;
    protected override bool IsThreatened(WorldState state) => state.FactoryUnderThreat;
    public override WorldState ApplyEffects(WorldState state)
    {
        state.FactoryUnderThreat = false;
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
