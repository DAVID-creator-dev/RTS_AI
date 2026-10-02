using AI.InfluenceMap;
using UnityEngine;

public class MoveToDefendLab : MoveToBase
{
    public override bool IsValid(WorldState state) => state.LabUnderThreat && !state.InPositionAtOwnLab;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.InPositionAtOwnLab = true;
        return state;
    }

    protected override Vector3 ResolveTarget(AIContext context, Squad squad)
    {
        Vector3 labPos = Vector3.zero;
        int worstInfluence = int.MaxValue;
        bool found = false;

        foreach (TargetBuilding lab in GameServices.GetTargetBuildings())
        {
            if (lab.GetTeam() != context.unitController.GetTeam())
                continue;

            int influence = context.influenceMap.GetSubMapInfluenceAtPosition<EnemyUnitInfluenceSubMap>(lab.transform.position);

            if (!found || influence < worstInfluence)
            {
                worstInfluence = influence;
                labPos         = lab.transform.position;
                found          = true;
            }
        }

        squad.DefenseTarget = labPos; 
        return labPos;
    }
}
