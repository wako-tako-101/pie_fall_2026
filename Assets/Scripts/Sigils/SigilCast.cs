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
            Debug.Log("Attempting cast");

            Vector3 castOrigin = castSource.position;
            Vector3 castDirection = castSource.forward;

            RaycastHit hit;
            if (Physics.Raycast(castOrigin, castDirection, out hit, 5f))
            {
                Debug.Log("Sending raycast");
                //get the object that was hit
                GameObject target = hit.collider.gameObject;
                
                //check if target is valid
                if (!target.GetComponent<SigilTagsMono>())
                {
                    Debug.Log("Object Name: " + target.name + " not interactable");
                }
                else if (target.GetComponent<SigilTagsMono>().CompareSigilTag(activeSigilType))
                {
                    Debug.Log("Object Name: " + target.name + ", Sigil Interaction: " + activeSigilType.ToString());
                }
            }
            else Debug.Log("No object hit by the cast");
        }
    }
}
