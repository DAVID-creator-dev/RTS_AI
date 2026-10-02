using System.Collections.Generic;
using UnityEngine;
using AI.InfluenceMap;

public class DestroyEnemyFactory : Goal
{
    public override WorldState GetGoalState()
    {
        return new WorldState { EnemyFactoryDestroyed = true };
    }

    public override int RatePriority(WorldState currentState, ETeam team, AIContext context = null)
    {
        if (!currentState.EnemyFactoryLocated)
            return priority = 0;

        return priority = 3;
    }
}
