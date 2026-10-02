using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MoveToDefendFactory : MoveToBase
{
    public override bool IsValid(WorldState state) => state.FactoryUnderThreat && !state.InPositionAtOwnFactory;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.InPositionAtOwnFactory = true;
        return state;
    }

   protected override Vector3 ResolveTarget(AIContext context, Squad squad)
    {
        Factory factory = context.unitController.GetFactoryList.FirstOrDefault(f => f != context.unitController.MainBase);
        
        if (factory != null)
        {
            squad.DefenseTarget = factory.transform.position; 
            return factory.transform.position; 
        }
        else
        {
            return squad.GetPosition();
        }
    }
}
