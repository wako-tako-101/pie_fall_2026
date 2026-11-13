using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] public UnityEvent SuccessfullPress;

    void OnTriggerEnter(Collider other)
    {
        if (gameObject.CompareTag(other.tag))
        {
            SuccessfullPress.Invoke();
        }
    }
}
