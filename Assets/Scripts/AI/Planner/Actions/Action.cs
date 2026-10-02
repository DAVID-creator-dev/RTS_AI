using System.Collections.Generic;
using UnityEngine;

public abstract class Action : MonoBehaviour
{
    //Action cost
    public float cost = 1f;

    //Action behaviour
    public abstract WorldState ApplyEffects(WorldState worldState);
    public abstract bool IsValid(WorldState state);
    public virtual bool Complete(Squad squad, AIContext context) => false;
    public virtual bool Abort(Squad squad, AIContext context) => false;
    public virtual void OnUpdate(Squad squad, AIContext context) { }

}