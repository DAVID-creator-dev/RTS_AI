using System.Collections.Generic;
using UnityEngine;

enum MoveType
{
    CLASSIC,
    CIRCLE
}

class FormationManager
{
    private readonly List<Unit> _units = new();
    private const float Spacing = 5f;

    public void AddUnit(Unit unit) => _units.Add(unit);

    public void RemoveUnit(Unit unit) => _units.Remove(unit);

    public int Count => _units.Count;

    public void MoveTo(Vector3 destination, MoveType moveType)
    {
        if (_units.Count == 0)
            return;

        Vector3 currentCenter = GetCenter();
        Vector3 direction = Vector3.forward;
        if ((destination - currentCenter).sqrMagnitude > 0.01f)
            direction = (destination - currentCenter).normalized;

        _units.Sort((a, b) => a.GetTypeId.CompareTo(b.GetTypeId));

        Vector3[] slots = moveType switch
        {
            MoveType.CLASSIC => ComputeGridSlots(destination, direction, _units.Count),
            MoveType.CIRCLE => ComputeGridSlotsCircle(destination, direction, _units.Count, 10f),
            _ => ComputeGridSlots(destination, direction, _units.Count),
        };

        for (int i = 0; i < _units.Count; i++)
            _units[i].OrderMove(slots[i]);
    }

    Vector3[] ComputeGridSlotsCircle(Vector3 center, Vector3 forward, int count, float radius)
    {
        Vector3[] slots = new Vector3[count];
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            slots[i] = center + offset * radius;
        }
        return slots;
    }

    Vector3[] ComputeGridSlots(Vector3 center, Vector3 forward, int count)
    {
        Vector3[] slots = new Vector3[count];
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(count));

        for (int i = 0; i < count; i++)
        {
            int row = i / cols;
            int col = i % cols;
            float colOffset = col - (cols - 1) * 0.5f;
            slots[i] = center + right * (colOffset * Spacing) - forward * (row * Spacing);
        }

        return slots;
    }

    public Vector3 GetCenter()
    {
        if (_units.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (Unit u in _units)
            sum += u.transform.position;

        return sum / _units.Count;
    }
}
