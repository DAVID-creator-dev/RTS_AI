using System.Collections.Generic;
using UnityEngine;
using AI.InfluenceMap;

public  class DefendLabG : Goal
{
    [SerializeField] private AnimationCurve EnemyInfluenceCurve;
    [SerializeField] private float influenceNormMax = 20f;

    public override WorldState GetGoalState()
    {
        return new WorldState { LabSecured = true };
    }

    public override int RatePriority(WorldState currentState, ETeam team, AIContext context = null)
    {
        if (!currentState.LabUnderThreat || context == null)
            return priority = 0;

        int mostNegative = 0;
        foreach (TargetBuilding lab in GameServices.GetTargetBuildings())
        {
            if (lab.GetTeam() != team) 
                continue;
            int inf = context.influenceMap.GetSubMapInfluenceAtPosition<EnemyUnitInfluenceSubMap>(lab.transform.position);
            if (inf < mostNegative) 
                mostNegative = inf;
        }

        float normalized = Mathf.Clamp01(Mathf.Abs(mostNegative) / Mathf.Max(1f, influenceNormMax));
        float score = EnemyInfluenceCurve.Evaluate(normalized);
        return priority = Mathf.Max(4, Mathf.RoundToInt(score * 10));
    }
}
