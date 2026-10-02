public class AttackEnemyFactory : AttackBase
{
    protected override bool IsInPosition(WorldState state) => state.InPositionAtEnemyFactory;
    protected override bool IsTargetLocated(WorldState state) => state.EnemyFactoryLocated;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.EnemyFactoryDestroyed = true;
        return state;
    }

    public override void OnUpdate(Squad squad, AIContext context)
    {
        UnitController enemy = GameServices.GetControllerByTeam(GameServices.GetOpponent(context.unitController.GetTeam()));

        Factory target = null;
        float bestDist = float.MaxValue;
        foreach (Factory factory in enemy.GetFactoryList)
        {
            if (factory == enemy.MainBase || !factory.IsAlive) continue;
            float d = (squad.GetPosition() - factory.transform.position).sqrMagnitude;
            if (d < bestDist) 
            { 
                bestDist = d; 
                target = factory; 
            }
        }

        if (target == null) return;

        foreach (Unit unit in squad.Units)
        {
            if (unit.NextTarget == target && unit.NextTarget.IsAlive)
                continue;

            unit.OrderAttack(target);
        }
    }

    public override bool Complete(Squad squad, AIContext context)
    {
        UnitController enemy = GameServices.GetControllerByTeam(GameServices.GetOpponent(context.unitController.GetTeam()));
        foreach (Factory factory in enemy.GetFactoryList)
        {
            if (factory == enemy.MainBase)
                 continue;
            if (factory.IsAlive) 
                return false;
        }
        return true;
    }
}
