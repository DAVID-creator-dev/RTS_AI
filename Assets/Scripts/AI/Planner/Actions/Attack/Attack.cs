using System.Collections.Generic;
using UnityEngine;

public abstract class AttackBase : Action
{
    public override bool IsValid(WorldState state) => IsInPosition(state) && IsTargetLocated(state);

    protected abstract bool IsInPosition(WorldState state);

    protected abstract bool IsTargetLocated(WorldState state);
}
