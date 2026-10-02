
using UnityEngine;

namespace AI.InfluenceMap
{
    public class InfluenceSubMap : ScriptableObject
    {
        protected InfluenceMap influenceMap;
        public Grid grid;

        public int updateFrame = 0;

        public Color[] colors { get; private set; }

        public virtual void Init(int width, int height)
        {
            colors = new Color[width*height];
        }

        public virtual void PopulateInfluenceMap(ETeam team)
        {
            
        }

        public virtual void PopulateDebugTexture(ETeam team)
        {

        }

        public void SetInfluenceMap(InfluenceMap map)
        {
            this.influenceMap = map;
        }

        public virtual void SetCell(int x, int y, int value)
        {
            if (!grid.Contains(x, y))
                return;
            grid.Values[x+y*influenceMap.width] = value;
            
        }

        public virtual void AddCell(int x, int y, int value)
        {
            if (!grid.Contains(x, y))
                return;
            grid.Values[x + y * influenceMap.width] += value;
        }
    }
}