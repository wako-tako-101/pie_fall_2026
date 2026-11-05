using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Script based off Prof. Stanbury's "Stick Walk" script
    //references
    public Transform head;
    public InputActionProperty moveStick;
    public float speed = 2f;
    private Vector2 movementInput;

    //controll variables
    void OnEnable()  => moveStick.action.Enable();
    void OnDisable() => moveStick.action.Disable();
    void Update()
    {
        //get head facing vectors, nullify verticality, and normalize
        Vector3 forward = head.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = head.right;
        right.y = 0f;
        right.Normalize();
        
        //calculate movement direction
        Vector3 direction = forward * movementInput.y + right * movementInput.x;

        //move the player
        transform.position += direction * speed * Time.deltaTime;
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //get controller input
            movementInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            //zero out movement input when the player stops moving
            movementInput = Vector2.zero;
        }
    }
}
