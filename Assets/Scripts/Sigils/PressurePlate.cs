using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] UnityEvent pressed;
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == gameObject.tag)
        {
            pressed.Invoke();
        }
    }
}
