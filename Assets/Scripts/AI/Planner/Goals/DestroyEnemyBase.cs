using System.Collections.Generic;
using UnityEngine;

public  class DestroyEnemyBase : Goal
{
    [SerializeField] private AnimationCurve strenghtCurve;

    public override WorldState GetGoalState()
    {
        return new WorldState { EnemyBaseDestroyed = true };
    }

    public override int RatePriority(WorldState currentState, ETeam team, AIContext context = null)
    {
        if (!currentState.EnemyBaseLocated)
            return priority = 0;

        int ownedLabs = 0, enemyLabs = 0;
        foreach (TargetBuilding lab in GameServices.GetTargetBuildings())
        {
            if (lab.GetTeam() == team) ownedLabs++;
            else if (lab.GetTeam() == GameServices.GetOpponent(team)) enemyLabs++;
        }
        float labRatio = ownedLabs / Mathf.Max(1f, enemyLabs);

        float score = strenghtCurve.Evaluate(labRatio);
        return priority = Mathf.Max(2, Mathf.RoundToInt(score * 8));
    }
}
