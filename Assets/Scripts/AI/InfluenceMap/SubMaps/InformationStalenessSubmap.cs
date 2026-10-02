using System.Collections;
using UnityEngine;

namespace AI.InfluenceMap
{

    [CreateAssetMenu(fileName = "InformationStalenessSubMap", menuName = "AI/Influence Map/Information Staleness")]
    /// <summary>
    /// Tracks how stale the information is on the map, as in, 
    /// how many update cycles its been since the information on a cell has last been updated
    /// </summary>
    public class InformationStalenessSubmap : InfluenceSubMap
    {
        public int maxStale = 100;

        public Color unknownColor;
        public Color minStaleColor;
        public Color maxStaleColor;

        public override void Init(int width, int height)
        {
            base.Init(width, height);
            grid = new Grid(width, height, 0);
        }

        public override void PopulateInfluenceMap(ETeam team)
        {
            
        }

        public override void PopulateDebugTexture(ETeam team)
        {
            for (int i = 0; i < grid.Size; ++i)
            {
                if(!influenceMap.IsVisible(i,(int)team) && !influenceMap.WasVisible(i,(int)team))
                {
                    colors[i] = unknownColor;
                    continue;
                }
                int staleness = influenceMap.updateCycle-grid.Get(i);
                float factor = (float)staleness / maxStale;

                Color color = Color.Lerp(minStaleColor, maxStaleColor, factor);
                
                colors[i] = color;
            }
        }

        public override void AddCell(int x, int y, int value)
        {
            if (!grid.Contains(x, y))
                return;
            grid.Values[x + y * influenceMap.width] += value;
        }

        public override void SetCell(int x, int y, int value)
        {
            if (!grid.Contains(x, y))
                return;
            grid.Values[x + y * influenceMap.width] = value;
        }
    }
}