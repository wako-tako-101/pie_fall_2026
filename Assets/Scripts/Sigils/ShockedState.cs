using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ObjectState))]
public class ShockedState : MonoBehaviour
{
    //event for shocked object interactions
    [field: SerializeField] UnityEvent ShockEvent;

    //method for changing state of an interactable into shocked for a duration of time
    public void ChangeState(float duration)
    {
        TryGetComponent(out ObjectState objState);
        if (objState != null)
        {
            objState.SetState(ObjectStateTags.States.Shocked);
            ShockEvent.Invoke();
            Debug.Log("Shocked state initiated for " + gameObject.name + " for " + duration + " seconds");
            StartCoroutine(ResetStateAfterDelay(duration));
        }
        else Debug.LogError("No ObjectState script attached to " + gameObject.name);
    }

    //reset state after a delay
    private System.Collections.IEnumerator ResetStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TryGetComponent(out ObjectState objState);
        if (objState != null)
        {
            objState.SetState(ObjectStateTags.States.Normal);
            Debug.Log("Shocked state ended for " + gameObject.name);
        }
        else Debug.LogError("No ObjectState script attached to " + gameObject.name);
    }
}
