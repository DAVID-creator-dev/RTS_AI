using AI.InfluenceMap;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public  class Goal : MonoBehaviour
{
    public virtual WorldState GetGoalState() => new WorldState();

    private List<Squad> assignedSquads = new();

    public bool isAssigned => assignedSquads != null && assignedSquads.Count > 0;
    public bool isActive => priority > 0 && isAssigned == false;
    public int priority { get; set; } = 0;

    // Override to cap the number of units this goal can receive (-1 = no cap)
    public virtual int MaxUnitCount => -1;

    /// <summary>
    /// Returns the squad currently working on this goal (we only ever assign one at a time), or null if none.
    /// </summary>
    public Squad GetAssignedSquad() => assignedSquads.Count > 0 ? assignedSquads[0] : null;

    public virtual int RatePriority(WorldState currentState, ETeam team, AIContext context = null)
    {
        return 0;
    }

    /// <summary>
    /// Returns true if this goal needs a new squad assigned to it.
    /// </summary>
    public virtual bool NeedsSquad(WorldState currentState, ETeam team)
    {
        if (assignedSquads.Count == 0)
            return true;
        return false;
    }

    public void AssignSquad(Squad squad)
    {
        if (!assignedSquads.Contains(squad))
        {
            assignedSquads.Add(squad);
        }
    }

    public void UnassignSquad(Squad squad)
    {
        if (assignedSquads.Contains(squad))
        {
            assignedSquads.Remove(squad);
        }
    }
}
