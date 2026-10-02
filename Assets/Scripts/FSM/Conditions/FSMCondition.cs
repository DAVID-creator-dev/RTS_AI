using UnityEngine;


[CreateAssetMenu(fileName = "FSMCondition",menuName = "FSM/FSMCondition")]
public class FSMCondition : ScriptableObject
{
    public virtual bool CheckCondition(FSM controller)
    {
        return true;
    }

}
