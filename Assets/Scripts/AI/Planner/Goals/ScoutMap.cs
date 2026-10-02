using UnityEngine;
using System.Collections.Generic;

public  class ScoutMap : Goal
{
    [SerializeField] private AnimationCurve TimeCurve;
    public override int MaxUnitCount => 1;

    public override WorldState GetGoalState()
    {
        return new WorldState { EnemyBaseLocated = true };
    }

    public override int RatePriority(WorldState currentState, ETeam team, AIContext context = null)
    {
        if (currentState.EnemyBaseLocated || currentState.EnemyLabLocated) 
            return priority = 0;
        
        return priority = 1;
    }
}
