using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private InputActionReference jumpAction;

    private void OnEnable()
    {
        if(jumpAction.action != null) jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        if(jumpAction.action != null) jumpAction.action.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if(jumpAction.action != null && jumpAction.action.WasPressedThisFrame())
        {
            Debug.Log("Jump Enabled!");
        }
    }
}
