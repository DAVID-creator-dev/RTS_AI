public class AttackEnemyBase : AttackBase
{
    protected override bool IsInPosition(WorldState state) => state.InPositionAtEnemyBase;
    protected override bool IsTargetLocated(WorldState state) => state.EnemyBaseLocated;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.EnemyBaseDestroyed = true;
        return state;
    }

    public override void OnUpdate(Squad squad, AIContext context)
    {
        UnitController enemy = GameServices.GetControllerByTeam(GameServices.GetOpponent(context.unitController.GetTeam()));
        if (enemy.MainBase == null || !enemy.MainBase.IsAlive) 
            return;

        foreach (Unit unit in squad.Units)
        {
            if (unit.NextTarget == enemy.MainBase && unit.NextTarget.IsAlive)
                continue;

            unit.OrderAttack(enemy.MainBase);
        }
    }

    public override bool Complete(Squad squad, AIContext context)
    {
        UnitController enemy = GameServices.GetControllerByTeam(GameServices.GetOpponent(context.unitController.GetTeam()));
        return enemy.MainBase == null || !enemy.MainBase.IsAlive;
    }
}
