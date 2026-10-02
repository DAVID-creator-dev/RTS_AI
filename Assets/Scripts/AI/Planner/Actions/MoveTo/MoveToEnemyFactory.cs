using UnityEngine;

public class MoveToEnemyFactory : MoveToBase
{
    public override bool IsValid(WorldState state) => state.EnemyFactoryLocated && !state.InPositionAtEnemyFactory;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.InPositionAtEnemyFactory = true;
        return state;
    }

    protected override Vector3 ResolveTarget(AIContext context, Squad squad)
    {
        ETeam opponent = GameServices.GetOpponent(context.unitController.GetTeam());
        UnitController enemyController = GameServices.GetControllerByTeam(opponent);

        Factory closest = null;
        float bestDist = float.MaxValue;
        foreach (Factory factory in enemyController.GetFactoryList)
        {
            if (factory == enemyController.MainBase) 
                continue;
            if (!factory.IsAlive) 
                continue;
            float d = Vector3.SqrMagnitude(squad.GetPosition() - factory.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = factory;
            }
        }

        return closest != null ? closest.transform.position : squad.GetPosition();
    }
}
