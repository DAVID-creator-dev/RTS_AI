using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AI.InfluenceMap
{
    public class InfluenceEmitter : MonoBehaviour
    {
        public ETeam team;
        public Vector2 position => new Vector2(transform.position.x, transform.position.z);

        public int influence = 5;
        public float range = 1f;

        public int GetInfluenceAtPos(Vector2 origin, Vector2 target, ETeam compareTeam)
        {
            float factor = Vector2.Distance(origin, target) / range;
            if (team == ETeam.Neutral)
                return 0;
            if (compareTeam != team)
            {
                return -Mathf.RoundToInt(influence * (1 - factor));
            }
            return Mathf.RoundToInt(influence * (1 - factor));
        }

        public int GetInfluenceAtPos(int decayedInfluence, Vector2 origin, Vector2 target, ETeam compareTeam)
        {
            float factor = Vector2.Distance(origin, target) / range;
            if (team == ETeam.Neutral)
                return 0;
            if (compareTeam != team)
            {
                return -Mathf.RoundToInt(decayedInfluence * (1 - factor));
            }
            return Mathf.RoundToInt(decayedInfluence * (1 - factor));
        }
    }
}
