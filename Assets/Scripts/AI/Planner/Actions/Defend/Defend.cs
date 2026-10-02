public abstract class DefendBase : Action
{
    public override bool IsValid(WorldState state) => IsInPosition(state) && IsThreatened(state);
    protected abstract bool IsInPosition(WorldState state);
    protected abstract bool IsThreatened(WorldState state);
}
