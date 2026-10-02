using AI.InfluenceMap;
using UnityEngine;

/// <summary>
/// Uses the InfluenceMap to assign a scouting priority to a map position based on an average of the surrounding area.
/// </summary>
public class ScoutPoint
{
    public float range = 15f;

    private InfluenceMap influenceMap;
    private InformationStalenessSubmap stalenessSubmap;
    private ETeam team;

    public Vector3 worldPos { get; private set; }
    private Vector2Int gridPos;

    public float stalenessFactor = 1f;
    public float influenceFactor = 1f;
    public float exploredFactor = 1f;

    public float priority { get; private set; } = 0f;

    private float averageStaleness = 0f;
    private float averageInfluence = 0f;
    private float averageExplored = 0f;

    private int minInfluence = 0;

    public int updateFrame = 0;

    public ScoutPoint(InfluenceMap influenceMap, ETeam team, Vector3 position)
    {
        Init(influenceMap, team, position);
        stalenessSubmap = influenceMap.stalenessSubmap;
    }

    public void Init(InfluenceMap influenceMap, ETeam team, Vector3 position)
    {
        this.influenceMap = influenceMap;
        this.team = team;
        worldPos = position;
        gridPos = influenceMap.GetPositionInGrid(worldPos);
        stalenessSubmap = influenceMap.GetSubMap<InformationStalenessSubmap>();
        minInfluence = influenceMap.GetSubMap<UnitInfluenceSubMap>().minInfluence;
    }

    public void ComputeAverages()
    {
        gridPos = influenceMap.GetPositionInGrid(worldPos);
        int radius = Mathf.FloorToInt(range / influenceMap.textureScale.x) - 1;
        if (radius <= 0)
            return;

        int count = 0;
        int totalStaleness = 0;
        int totalInfluence = 0;
        int totalExplored = 0;

        for (int dy = -radius; dy <= radius; ++dy)
        {
            int y = gridPos.y + dy;
            int dx = Mathf.FloorToInt(Mathf.Sqrt(radius * radius - dy * dy));
            int xMin = gridPos.x - dx;
            int xMax = gridPos.x + dx;

            

            for (int x = xMin; x <= xMax; ++x)
            {
                if (!influenceMap.Contains(x, y))
                    continue;
                bool isVisible = influenceMap.IsVisible(x, y, (int)team);
                bool wasVisible = influenceMap.WasVisible(x, y, (int)team);

                count++;
                totalInfluence += influenceMap.GetInfluenceAtPosition(x,y);
                totalStaleness += influenceMap.GetStalenessAtPosition(x,y);
                totalExplored += isVisible ? 0 : wasVisible ? 1 : 2;
            }
        }

        averageStaleness = (float)totalStaleness / count;
        averageInfluence = (float)totalInfluence / count;
        averageExplored = (float)totalExplored / count;

        /* Compute Priority:
         * The more stale, the higher the priority,
         * The less explored, the higher the priority.
         * If influence is negative (enemy presence), the priority should be lower.
        */

        float stalenessWeight = Mathf.Lerp(0f, 1f, averageStaleness / stalenessSubmap.maxStale) * stalenessFactor;
        float exploredWeight = Mathf.Lerp(0f, 1f, averageExplored / 2f) * exploredFactor; // 0 = fully explored, 1 = partially explored, 2 = unexplored
        float influenceWeight = averageInfluence < 0 ? Mathf.LerpUnclamped(0f, -2f, averageInfluence / minInfluence) * influenceFactor : 0f; // if influence is negative, reduce priority

        priority = stalenessWeight+exploredWeight+influenceWeight;
    }

}
