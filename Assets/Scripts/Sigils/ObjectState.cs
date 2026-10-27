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
       /*
       //event for shock object ineractions
       [field: SerializeField] ShockEvent;

       //method for changing state of an interactable into shocked for a duration of time
       ChangeState (duration t)
       {
            ObjectState.state = Shocked;
            ShockEvent.Invoke();
            Delay(t);
            ObjectState.state = Normal;
       }
       */ 
    }
    public class LevitateState
    {
        /*
        //bool to determine if still under sigil effect
        bool windLifted;
        //method for childing object to player controller or moving object in sync with controller
        ChangeState()
        {
            if (!windLifted)
            {
                windLifted = true;
                ObjectState.state = WindControlled;
            }
            else
            {
                windLifted = false;
                ObjectState.state = Normal;
            }
        }
        Update()
        {
            while(windLifted)
            {
                SyncMovement();
            }
        }
        SyncMovement()
        {
            //responsible for setting position of object to pointer of controller
            //or childing to player depending on desired effect
        }
        */
    }
}
