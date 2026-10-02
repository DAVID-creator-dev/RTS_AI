using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine; 

class Explore : Action
{
    public override WorldState ApplyEffects(WorldState worldState)
    {
        worldState.EnemyLabLocated = true;
        worldState.EnemyFactoryLocated = true;
        worldState.EnemyBaseLocated = true;
        return worldState;
    }

    public override bool IsValid(WorldState state)
    {
        return !state.EnemyLabLocated || !state.EnemyFactoryLocated || !state.EnemyBaseLocated;
    }

    public override void OnUpdate(Squad squad, AIContext context)
    {
        if (squad.ExploreTarget != null)
            return;

        squad.ExploreTarget = context.scoutingSubSystem.GetBestScoutingPoint(squad.GetPosition());
        squad.MoveFormation(squad.ExploreTarget.worldPos);
    }

    public override bool Complete(Squad squad, AIContext context)
    {
        if (!squad.HasReachedPos())
            return false;

        squad.ExploreTarget = null;
        return true;
    }
}