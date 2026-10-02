using UnityEngine;

namespace AI.InfluenceMap
{
    [CreateAssetMenu(fileName = "EnemyUnitInfluenceSubMap", menuName = "AI/Influence Map/Enemy Unit Influence Sub Map")]
    public class EnemyUnitInfluenceSubMap : UnitInfluenceSubMap
    {
        protected override bool ShouldTrack(EntityInfluence v, ETeam team) => v.team != team;
    }
}
