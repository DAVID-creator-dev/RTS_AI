using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.InfluenceMap
{
    [CreateAssetMenu(fileName = "InfluenceSubMap", menuName = "AI/Influence Map/Unit Influence Sub Map")]
    public class UnitInfluenceSubMap : InfluenceSubMap
    {
        /// <summary>
        /// class containing the influence a unit currently holds on the map
        /// It is class to have pass-by-ref behaviour
        /// </summary>
        public class SpottedUnit
        {
            public SpottedUnit(int pos, EntityInfluence influence)
            {
                this.pos = pos;
                this.influence = influence;
                decayTime = 0;
                decayedInfluence = influence.influence;
            }

            public int pos;
            public EntityInfluence influence;
            public float decayTime; //time its been decaying for
            public int decayedInfluence;
        }

        public int minInfluence = -10;
        public int maxInfluence = 10;

        public Color defaultColor;
        public Color maxInfluenceColor;
        public Color minInfluenceColor;

        public float decayStartTime = 15f; //time after which a unit's influence starts to decay if not visible
        public float decayTime = 30f; //time after which the unit's influence will be fully decayed
        private float removeTime = 0f;

        List<SpottedUnit> spottedUnits = new();

        public override void Init(int width, int height)
        {
            base.Init(width, height);
            grid = new Grid(width, height, 0);
            removeTime = decayStartTime + decayTime;
        }

        public override void PopulateInfluenceMap(ETeam team)
        {
            UpdateVisions(FindObjectsByType<EntityInfluence>(FindObjectsSortMode.None), (ETeam)team);
        }

        protected virtual bool ShouldTrack(EntityInfluence v, ETeam team) => true;

        public void UpdateVisions(EntityInfluence[] influences, ETeam team)
        {
            grid.Clear();
            foreach (EntityInfluence v in influences)
            {
                if (!ShouldTrack(v, team))
                    continue;

                Vector2Int gridPos = influenceMap.GetPositionInGrid(v.position);
                int pos = gridPos.x + gridPos.y * influenceMap.width;

                bool isVisible = influenceMap.IsVisible(pos, (int)team);
                bool wasVisible = influenceMap.WasVisible(pos, (int)team);

                SpottedUnit unit = GetSpottedUnit(v, pos);

                //ignore enemies we cant see
                if (  !isVisible && (!wasVisible || unit == null))
                    continue;
            
                if (isVisible)
                {
                    if(unit == null)
                        unit = AddSpottedUnit(v, pos);
                    unit.decayTime = 0f; //reset decay time if unit was visible
                }
                else if(wasVisible && DecayInfluence(unit)) //if the unit was visible but is no longer, slowly decay its influence
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
                        AddCell(x, y, v.GetInfluenceAtPos(unit.decayedInfluence, gridPos, new Vector2(x, y), team));
                        if(v.team == team)
                        {
                            influenceMap.SetStalenessAtPosition(x, y);
                        }
                    }
                }
            }
        }

        public SpottedUnit GetSpottedUnit(EntityInfluence influence, int pos)
        {
            foreach (SpottedUnit spotted in spottedUnits)
            {
                if (spotted.influence == influence)
                    return spotted;
            }
            return null;
        }

        public SpottedUnit AddSpottedUnit(EntityInfluence influence, int pos)
        {
            SpottedUnit newSpotted = new SpottedUnit(pos, influence);
            spottedUnits.Add(newSpotted);
            return newSpotted;
        }

        /// <summary>
        /// Decays the influence of units that are no longer visible on the map. Returns a boolean that is true if the unit has fully decayed
        /// </summary>
        public bool DecayInfluence(SpottedUnit a)
        {
            a.decayTime += Time.deltaTime;
            if (a.decayTime > removeTime)
            {
                spottedUnits.Remove(a);
                return true;
            }
            if(a.decayTime > decayStartTime)
            {
                float decayPercent = (a.decayTime - decayStartTime) / decayTime;
                a.decayedInfluence = Mathf.RoundToInt(Mathf.Lerp(a.influence.influence, 0, decayPercent));
            }
            return false;
        }


        public override void PopulateDebugTexture(ETeam team)
        {
            for (int i = 0; i < grid.Size; ++i)
            {
                int influence = grid.Get(i);

                Color color = defaultColor;
                if(influence<0)
                    color = Color.Lerp(defaultColor, minInfluenceColor, Mathf.Abs((float)influence / minInfluence));
                else if (influence > 0)
                    color = Color.Lerp(defaultColor, maxInfluenceColor, Mathf.Abs((float)influence / maxInfluence));

                colors[i] = color;
            }
        }
    }
}