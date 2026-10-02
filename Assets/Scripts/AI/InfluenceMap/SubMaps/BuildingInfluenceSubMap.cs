using UnityEngine;

namespace AI.InfluenceMap
{
    [CreateAssetMenu(fileName = "BuildingInfluenceSubMap", menuName = "AI/Influence Map/Building Influence Sub Map")]
    public class BuildingInfluenceSubMap : InfluenceSubMap
    {
        public int minInfluence = -10;
        public int maxInfluence = 10;

        public Color defaultColor;
        public Color maxInfluenceColor;
        public Color minInfluenceColor;

        public override void Init(int width, int height)
        {
            base.Init(width, height);
            grid = new Grid(width, height, 0);
        }

        public override void PopulateInfluenceMap(ETeam team)
        {
            UpdateVisions(FindObjectsByType<BuildingInfluence>(FindObjectsSortMode.None), team);
        }

        public void UpdateVisions(BuildingInfluence[] influences, ETeam team)
        {
            grid.Clear();
            foreach (BuildingInfluence v in influences)
            {
                Vector2Int gridPos = influenceMap.GetPositionInGrid(v.position);
                int pos = gridPos.x + gridPos.y * influenceMap.width;

                bool isVisible = influenceMap.IsVisible(pos, (int)team);
                bool wasVisible = influenceMap.WasVisible(pos, (int)team);

                //ignore enemies we cant see
                if (!isVisible && !wasVisible)
                    continue;

                int radius = Mathf.FloorToInt(v.range / influenceMap.textureScale.x) - 1;
                if (radius <= 0)
                    continue;

                for (int dy = -radius; dy <= radius; ++dy)
                {
                    int y = gridPos.y + dy;
                    int dx = Mathf.FloorToInt(Mathf.Sqrt(radius * radius - dy * dy));
                    int xMin = gridPos.x - dx;
                    int xMax = gridPos.x + dx;

                    for (int x = xMin; x <= xMax; ++x)
                    {
                        AddCell(x, y, v.GetInfluenceAtPos(gridPos, new Vector2(x, y), team));
                        if (v.team == team)
                        {
                            influenceMap.SetStalenessAtPosition(x, y);
                        }
                    }
                }
            }
        }

        public override void PopulateDebugTexture(ETeam team)
        {
            for (int i = 0; i < grid.Size; ++i)
            {
                int influence = grid.Get(i);

                Color color = defaultColor;
                if (influence < 0)
                    color = Color.Lerp(defaultColor, minInfluenceColor, Mathf.Abs((float)influence / minInfluence));
                else if (influence > 0)
                    color = Color.Lerp(defaultColor, maxInfluenceColor, Mathf.Abs((float)influence / maxInfluence));

                colors[i] = color;
            }
        }
    }
}