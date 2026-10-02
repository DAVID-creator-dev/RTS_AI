using System.Collections.Generic;
using UnityEngine;

public class SquadProduction : MonoBehaviour
{
    [System.Serializable]
    public struct SquadRatio
    {
        [Range(0f, 1f)] public float trooper;
        [Range(0f, 1f)] public float trike;
        [Range(0f, 1f)] public float quad;
        [Range(0f, 1f)] public float tank;
        [Range(0f, 1f)] public float tankHeavyA;
        [Range(0f, 1f)] public float tankHeavyB;
    }

    [SerializeField] SquadRatio _ratio = new()
    {
        trooper    = 0.3f,
        trike      = 0.15f,
        quad       = 0.15f,
        tank       = 0.25f,
        tankHeavyA = 0.07f,
        tankHeavyB = 0.08f,
    };

    [Header("Unit Type IDs (match UnitDataScriptable TypeId)")]
    [SerializeField] int _trooperTypeId    = 0;
    [SerializeField] int _trikeTypeId      = 1;
    [SerializeField] int _quadTypeId       = 2;
    [SerializeField] int _tankTypeId       = 3;
    [SerializeField] int _tankHeavyATypeId = 4;
    [SerializeField] int _tankHeavyBTypeId = 5;

    /// <summary>
    /// Calculate the deficit for each unit. The unit with the most negative deficit relative to its ratio is the one to be created. 
    /// </summary>
    public int GetMostNeededTypeId(Factory factory, UnitController unitController)
    {
        var buildableTypeIds = new HashSet<int>();
        for (int i = 0; i < factory.AvailableUnitsCount; i++)
        {
            UnitDataScriptable data = factory.GetBuildableUnitData(i);
            if (data != null) 
                buildableTypeIds.Add(data.TypeId);
        }

        int totalUnits = unitController.UnitList.Count;
        foreach (Factory f in unitController.GetFactoryList)
            totalUnits += f.PendingUnitCount;

        int   bestTypeId  = -1;
        float bestDeficit = float.MinValue;

        UpdateBest(ref bestTypeId, ref bestDeficit, buildableTypeIds, unitController, totalUnits, _trooperTypeId,    _ratio.trooper);
        UpdateBest(ref bestTypeId, ref bestDeficit, buildableTypeIds, unitController, totalUnits, _trikeTypeId,      _ratio.trike);
        UpdateBest(ref bestTypeId, ref bestDeficit, buildableTypeIds, unitController, totalUnits, _quadTypeId,       _ratio.quad);
        UpdateBest(ref bestTypeId, ref bestDeficit, buildableTypeIds, unitController, totalUnits, _tankTypeId,       _ratio.tank);
        UpdateBest(ref bestTypeId, ref bestDeficit, buildableTypeIds, unitController, totalUnits, _tankHeavyATypeId, _ratio.tankHeavyA);
        UpdateBest(ref bestTypeId, ref bestDeficit, buildableTypeIds, unitController, totalUnits, _tankHeavyBTypeId, _ratio.tankHeavyB);

        return bestTypeId;
    }

    void UpdateBest(ref int bestTypeId, ref float bestDeficit, HashSet<int> buildableTypeIds, UnitController unitController, int totalUnits, int typeId, float ratio)
    {
        if (!buildableTypeIds.Contains(typeId))
            return;

        int current = 0;
        foreach (Unit u in unitController.UnitList)
            if (u.GetTypeId == typeId) current++;
        foreach (Factory f in unitController.GetFactoryList)
            current += f.GetPendingCountByTypeId(typeId);

        float deficit = ratio * totalUnits - current;
        if (deficit > bestDeficit)
        {
            bestDeficit = deficit;
            bestTypeId  = typeId;
        }
    }
}
