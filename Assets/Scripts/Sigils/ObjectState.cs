using System;
using UnityEngine;

public class ObjectStateTags
{
    public enum States {Normal, Frozen, Shocked, WindControlled}
    
}
public class ObjectState : MonoBehaviour
{
    //references
    Rigidbody objectRB;
    SigilTagsMono sigTag;
    //system
    ObjectStateTags.States state = ObjectStateTags.States.Normal;
    //functions
    void Start()
    {
        //obtain rigidbody
        if (TryGetComponent(out objectRB)){}
        else Debug.LogWarning("No rigidbody detected for " + gameObject.name);

        //obtain sigil tag
        if (TryGetComponent(out sigTag)){}
        else Debug.LogError("No sigil tag associated with " + gameObject.name);
    }
    public class ShockedState
    {
        
    }
    public class LevitateState
    {
    }
}
