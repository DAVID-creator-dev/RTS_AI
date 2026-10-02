
using UnityEngine;

namespace AI.InfluenceMap
{
    [CreateAssetMenu(fileName = "InfluenceSubMap", menuName = "AI/Influence Map/Fog Of War Influence Sub Map")]
    public class FogOfWarInfluenceSubMap : InfluenceSubMap
    {
        private FogOfWarSystem fogSystem;

        public Color startColor;

        public override void Init(int width, int height) 
        {
            base.Init(width,height);
            fogSystem = FindFirstObjectByType<FogOfWarSystem>();
            grid = fogSystem.VisibilityGrid;
        }

        public override void PopulateInfluenceMap(ETeam team)
        {
            
        }

        public override void PopulateDebugTexture(ETeam team)
        {
            Grid visibilityGrid = fogSystem.VisibilityGrid;
            Grid previousVisibilityGrid = fogSystem.PreviousVisibilityGrid;

            Color startColor = new Color(0f, 1f, 0f, 0.5f);
            Color endColor = new Color(1f, 0f, 0f, 0.5f);

            int teamNum = team == ETeam.Red ? 0 : 1;

            for (int i = 0; i < grid.Size; ++i)
            {
                bool isVisible = (grid.Get(i) & teamNum) == teamNum;
                bool wasVisible = (previousVisibilityGrid.Get(i) & teamNum) == teamNum;

                float value = isVisible ? 1f : wasVisible ? 0.5f : 0;

                Color newColor = Color.Lerp(startColor, endColor, value);

                colors[i] = newColor;
            }
        }

        public bool IsVisible(int a, int team)
        {
            team = 1 << team;
            if (grid.Size < a || a < 0)
                return false;
            return (grid.Get(a) & team) == team;
        }

        public bool WasVisible(int a, int team)
        {
            team = 1 << team;
            if (grid.Size < a || a < 0)
                return false;
            return (fogSystem.PreviousVisibilityGrid.Get(a) & team) == team;
        }

    }
}