using UnityEngine;

public class MoveToLab : MoveToBase
{
    public override bool IsValid(WorldState state) => state.EnemyLabLocated && !state.InPositionAtEnemyLab;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.InPositionAtEnemyLab = true;
        return state;
    }

    protected override Vector3 ResolveTarget(AIContext context, Squad squad)
    {        
        TargetBuilding tb = null;
        float closestDistance = float.MaxValue;

        foreach (TargetBuilding lab in GameServices.GetTargetBuildings())
        {
            if (lab.GetTeam() == context.unitController.GetTeam())
                continue;

            if (!context.influenceMap.WasVisible(lab.transform.position, (int)context.unitController.GetTeam()))
                continue;

            float dist = Vector3.Distance(squad.GetPosition(), lab.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                tb = lab;
            }
        }

        if (tb != null)
        {
            squad.CaptureTarget = tb; 
            return tb.transform.position; 
        }
        else
        {
            return squad.GetPosition(); 
        }
    }
}
