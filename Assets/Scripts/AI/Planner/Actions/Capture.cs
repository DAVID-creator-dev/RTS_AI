using UnityEngine;

public class Capture : Action
{
    public override bool IsValid(WorldState state) => state.InPositionAtEnemyLab && state.EnemyLabLocated;

    public override WorldState ApplyEffects(WorldState state)
    {
        state.LabCaptured          = true;
        state.AllEnemyLabsCaptured = true;  
        return state;
    }

    public override bool Complete(Squad squad, AIContext context)
    {
        if (squad.CaptureTarget == null) 
            return false;

        return squad.CaptureTarget.GetTeam() == context.unitController.GetTeam();
    }

    public override bool Abort(Squad squad, AIContext context)
    {
        foreach (Unit unit in squad.Units)
            unit.StopCapture();
        return true;
    }

    public override void OnUpdate(Squad squad, AIContext context)
    {
        if (squad.CaptureTarget == null) 
            return;

        foreach (Unit unit in squad.Units)
            if (!unit.IsCapturing())
                unit.OrderCapture(squad.CaptureTarget);
    }
}
