using AI.InfluenceMap;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public Action<ETeam> OnGameOver;
    public ETeam FirstTeamColor = ETeam.Neutral;
    public ETeam SecondTeamColor = ETeam.Neutral;
    public bool IsGameOver { get; private set; }
    public static GameState Instance { get; private set; }
    #region Team and scoring methods

    public InfluenceMap InfluenceMap;

    // Scores are based on the number of factories per team
    int[] TeamScores = new int[2];
    public int[] GetTeamScores { get { return TeamScores; } }
    public ETeam GetOpponent(ETeam team)
    {
        if (team == FirstTeamColor)
            return SecondTeamColor;
        return FirstTeamColor;
    }
    public void IncreaseTeamScore(ETeam team)
    {
        if (team >= ETeam.Neutral)
            return;

        ++TeamScores[(int)team];
    }
    public void DecreaseTeamScore(ETeam team)
    {
        if (team >= ETeam.Neutral)
            return;

        --TeamScores[(int)team];

        if (TeamScores[(int)team] <= 0)
            OnGameOver(GetOpponent(team));
    }

    public List<TargetBuilding> GetDiscoveredEnemyLabs(ETeam team, InfluenceMap influenceMap)
    {
        List<TargetBuilding> result = new();
        foreach (TargetBuilding lab in GetAllEnemyLabs(team))
        {
            if (influenceMap.WasVisible(lab.transform.position, (int)team))
                result.Add(lab);
        }
        return result;
    }

    public List<TargetBuilding> GetAllEnemyLabs(ETeam team)
    {
        List<TargetBuilding> EnemyLabs = new List<TargetBuilding>();

        foreach(var Lab in GameServices.GetTargetBuildings())
        {
            if (team == ETeam.Neutral)
                continue;

            if (team != Lab.GetTeam())
                EnemyLabs.Add(Lab);
        }

        return EnemyLabs;
    }

    public List<TargetBuilding> GetAllOwnedLabs(ETeam team)
    {
        List<TargetBuilding> OwnedLabs = new List<TargetBuilding>();

        foreach (var Lab in GameServices.GetTargetBuildings())
        {
            if (team == ETeam.Neutral)
                continue;

            if (team == Lab.GetTeam())
                OwnedLabs.Add(Lab);
        }

        return OwnedLabs;
    }

    public TargetBuilding GetNearestLab(Vector3 pos)
    {
        TargetBuilding nearest = null;
        float bestDist = float.MaxValue;
        foreach (TargetBuilding target in GameServices.GetTargetBuildings())
        {
            if (target.GetTeam() != ETeam.Neutral)
                continue;
            float dist = Vector3.Distance(pos, target.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = target;
            }
        }
        return nearest;
    }
    
    public Factory GetMyBase(ETeam team)
    {
        var factories = GameServices.GetControllerByTeam(team).GetFactoryList;
        return factories.Count > 0 ? factories[0] : null;
    }

    public Vector3? GetEnemyBasePosition(ETeam team)
    {
        var factories = GameServices.GetControllerByTeam(GameServices.GetOpponent(team)).GetFactoryList;
        return factories.Count > 0 ? factories[0].transform.position : (Vector3?)null;
    }

    public float GetCheapestUnit(ETeam team)

    {
        List<Unit> units = GameServices.GetControllerByTeam(team).UnitList;
        float cheapestCost = 1000.0f;

        foreach (var unit in units)
        {
            if (unit.Cost < cheapestCost)
            {
                cheapestCost = unit.Cost;
            }
        }

        return cheapestCost;
    }

    public float GetTeamHealth(ETeam team)
    {
        List<Unit> units = GameServices.GetControllerByTeam(team).UnitList;

        if (units.Count == 0)
            return 1f;

        float sum = 0f;

        foreach (var unit in units)
            sum += unit.GetHP() / (float)unit.GetUnitData.MaxHP;

        return sum / units.Count;
    }

    public float GetGameTime()
        => Time.time;
    #endregion

    #region MonoBehaviour methods
    void Start()
    {
        Instance = this;
    }

    #endregion
}
