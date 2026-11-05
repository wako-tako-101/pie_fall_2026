using UnityEngine;
using UnityEngine.InputSystem;

public class SigilCast : MonoBehaviour
{
    //system
    [SerializeField] Transform castSource;
    [SerializeField] SigilTags.SigilTag activeSigilType;
    public void OnCast(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 castOrigin = castSource.position;
            Vector3 castDirection = castSource.forward;

            RaycastHit hit;
            if (Physics.Raycast(castOrigin, castDirection, out hit, 5f))
            {
                //get the object that was hit
                GameObject target = hit.collider.gameObject;
                
                //check if target is valid
                if (target.TryGetComponent(out SigilTagsMono sigilTag) 
                    && sigilTag.CompareSigilTag(activeSigilType))
                {
                    //get object state and initiate sigil interaction
                    if (target.TryGetComponent(out ObjectState objState)) 
                    {
                        objState.Interact(gameObject, activeSigilType);
                    }
                    else Debug.LogError("No ObjectState script attached to " + target.name);
                }
                else Debug.Log("Target is not a valid sigil target");
            }
            else Debug.Log("No object hit by sigil cast");
        }
    }
}
