using System.Collections.Generic;
using UnityEngine;

public  class CaptureLab : Goal
{
    [SerializeField] private AnimationCurve EnemyInfluenceCurve;
    [SerializeField] private AnimationCurve MoneyCurve;
    public override int MaxUnitCount => 3;

    public override WorldState GetGoalState()
    {
        return new WorldState { AllEnemyLabsCaptured = true };
    }

    public override int RatePriority(WorldState currentState, ETeam team, AIContext context = null)
    {
        if (!currentState.EnemyLabLocated || currentState.AllEnemyLabsCaptured)
            return priority = 0;

        return priority = 2; 
    }
}
