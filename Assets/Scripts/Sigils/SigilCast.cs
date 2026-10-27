using UnityEngine;
using UnityEngine.InputSystem;

public class SigilCast : MonoBehaviour
{
    //objects
    Collider castBox;
    GameObject target;
    //system
    [SerializeField] SigilTags.SigilTag activeSigilType;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<SigilTagsMono>(out var objectTag))
        {
            if (objectTag.SigilTag != activeSigilType) return;
            else target = other.gameObject;
        }
    }
    public void OnCast(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!target.GetComponent<SigilTagsMono>()) return;
            else if (target.GetComponent<SigilTagsMono>().CompareSigilTag(activeSigilType))
            {
                Debug.Log("Object Name: " + target.name + ", Sigil Interaction: " + activeSigilType.ToString());
            }
        }
    }
}
