using System.Collections.Generic;
using UnityEngine;
using AI.InfluenceMap;

public  class DefendBaseG : Goal
{
    [SerializeField] private AnimationCurve HealthCurve;
    [SerializeField] private AnimationCurve EnemyInfluenceCurve;
    [SerializeField] private float influenceNormMax = 20f;

    public override WorldState GetGoalState()
    {
        return new WorldState { BaseAlive = true };
    }

    public override int RatePriority(WorldState currentState, ETeam team, AIContext context = null)
    { 
        if (!currentState.BaseUnderThreat || context == null)
            return priority = 0;
        
        Factory mainBase = context.unitController.MainBase;
        float healthRatio = (float)mainBase.GetHP() / mainBase.GetFactoryData.MaxHP;

        int influence = context.influenceMap.GetSubMapInfluenceAtPosition<EnemyUnitInfluenceSubMap>(mainBase.transform.position);
        float normalized = Mathf.Clamp01(Mathf.Abs(influence) / Mathf.Max(1f, influenceNormMax));

        float score = HealthCurve.Evaluate(healthRatio) + EnemyInfluenceCurve.Evaluate(normalized);
        return priority = Mathf.Max(6, Mathf.RoundToInt(score * 10));
    }
}
