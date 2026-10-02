using System.Collections.Generic;
using UnityEngine;

namespace AI.Planner
{
    public class ScoutingSubSystem : MonoBehaviour
    {
        private ETeam team;
        private List<ScoutPoint> points = new();
        private InfluenceMap.InfluenceMap influenceMap;

        public int updateFrequency = 5;
        private int updateFrameCount = 0;

        public float influenceDecayDistance = 150f; // The distance at which the influence of a point decays

        public Transform debugSampleTransform = null;

        public float debugMinPriority = 0f;
        public float debugMaxPriority = 2f;

        public Color debugMinPriorityColor;
        public Color debugMaxPriorityColor;

        public void Setup(ETeam team)
        {
            this.team = team;
            influenceMap = FindFirstObjectByType<InfluenceMap.InfluenceMap>();
            SetupPoints(team);
        }

        // Update is called once per frame
        void Update()
        {
            updateFrameCount++;

            if (updateFrameCount >= updateFrequency)
            {
                updateFrameCount = 0;
            }
            foreach (ScoutPoint point in points)
            {
                if (point.updateFrame == updateFrameCount)
                {
                    point.ComputeAverages();
                }
            }
        }

        void SetupPoints(ETeam team)
        {
            this.team = team;
            Factory[] factories = FindObjectsByType<Factory>(FindObjectsSortMode.None);
            TargetBuilding[] targetBuildings = FindObjectsByType<TargetBuilding>(FindObjectsSortMode.None);

            foreach (Factory factory in factories)
            {
                ScoutPoint point = new ScoutPoint(influenceMap, team, factory.transform.position);
                points.Add(point);
            }
            foreach (TargetBuilding targetBuilding in targetBuildings)
            {
                ScoutPoint point = new ScoutPoint(influenceMap, team, targetBuilding.transform.position);
                points.Add(point);
            }
            SetupStaggeredUpdate();
        }

        void SetupStaggeredUpdate()
        {
            for (int i = 0; i < points.Count; ++i)
            {
                points[i].updateFrame = (i % (updateFrequency - 1));
            }
        }

        public ScoutPoint GetBestScoutingPoint(Vector3 position)
        {
            ScoutPoint bestPoint = null;
            float bestPointScore = float.MinValue;
            foreach (ScoutPoint point in points)
            {
                float score = point.priority - Vector3.Distance(position, point.worldPos) / influenceDecayDistance; // Adjust the weight as needed
                if(score > bestPointScore) 
                {
                    bestPointScore = score;
                    bestPoint = point;
                }
            }
            return bestPoint;
        }

        private void OnDrawGizmos()
        {
            foreach(ScoutPoint point in points) 
            {
                float score;
                if(debugSampleTransform)
                    score = point.priority - Vector3.Distance(debugSampleTransform.position, point.worldPos) / influenceDecayDistance;
                else
                    score = point.priority;
                Color pointColor = Color.Lerp(debugMinPriorityColor, debugMaxPriorityColor, score / debugMaxPriority);
                Gizmos.color = pointColor;
                Gizmos.DrawSphere(point.worldPos, 10f);
            }
        }
    }
}