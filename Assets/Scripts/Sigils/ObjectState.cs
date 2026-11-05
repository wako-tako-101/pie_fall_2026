using UnityEngine;

public class ObjectStateTags
{
    public enum States {Normal, Frozen, Shocked, WindControlled}
    
}

[RequireComponent(typeof(SigilTagsMono))]
public class ObjectState : MonoBehaviour
{
    //system
    ObjectStateTags.States state = ObjectStateTags.States.Normal;
    //functions
    void Start()
    {
        //obtain rigidbody
        if (TryGetComponent(out Rigidbody objectRB)){}
        else Debug.LogWarning("No rigidbody detected for " + gameObject.name);

        //obtain sigil tag
        if (TryGetComponent(out SigilTagsMono sigTag)){}
        else Debug.LogError("No sigil tag associated with " + gameObject.name);
    }
    public ObjectStateTags.States GetState()
    {
        return state;
    }
    public void SetState(ObjectStateTags.States newState)
    {
        state = newState;
    }
    public void Interact(GameObject caster, SigilTags.SigilTag sigilTag)
    {
        switch (sigilTag)
        {
            case SigilTags.SigilTag.Levitate:
                if (TryGetComponent(out LevitateState levitateState))
                {
                    Debug.Log("Levitate state initiated for " + gameObject.name);
                    levitateState.ChangeState(caster);
                }
                else Debug.LogError("No LevitateState script attached to " + gameObject.name);
                break;
            case SigilTags.SigilTag.Push:
                break;
            case SigilTags.SigilTag.Freeze:
                break;
            case SigilTags.SigilTag.Burn:
                break;
            case SigilTags.SigilTag.Electricute:
                if (TryGetComponent(out ShockedState shockedState))
                {
                    Debug.Log("Shocked state initiated for " + gameObject.name);
                    shockedState.ChangeState(3f);//find solution for magic number later
                }
                else Debug.LogError("No ShockedState script attached to " + gameObject.name);
                break;
            default:
                Debug.LogError("Invalid sigil tag for " + gameObject.name);
                break;
        }
    }
}
