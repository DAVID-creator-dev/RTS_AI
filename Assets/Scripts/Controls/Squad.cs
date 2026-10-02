using System.Collections.Generic;
using UnityEngine;

public class Squad
{
    private readonly List<Unit> units = new();
    public IReadOnlyList<Unit> Units => units;
    private readonly FormationManager formation = new();
    public Goal assignedGoal;
    public Queue<Action> Plan = new();
    public TargetBuilding CaptureTarget;
    public Vector3 DefenseTarget;
    public ScoutPoint ExploreTarget;
    public Vector3? MoveToTarget;
    public int Count => units.Count;

    public void AddUnit(Unit unit)
    {
        if (units.Contains(unit))
            return;

        unit.CurrentSquad = this;
        units.Add(unit);
        formation.AddUnit(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        unit.CurrentSquad = null;
        units.Remove(unit);
        formation.RemoveUnit(unit);
    }

    public void MoveFormation(Vector3 destination) => formation.MoveTo(destination, MoveType.CLASSIC);

    public void MoveCircleFormation(Vector3 destination) => formation.MoveTo(destination, MoveType.CIRCLE);

    public bool HasReachedPos()
    {
        foreach (Unit u in units)
            if (!u.HasReachedDestination())
                return false;
        
        return true;
    }

    public Vector3 GetPosition()
    {
        if (units.Count == 0) 
            return Vector3.zero;
        
        Vector3 sum = Vector3.zero;
        foreach (Unit u in units) 
            sum += u.transform.position;
        
        return sum / units.Count;
    }
}
