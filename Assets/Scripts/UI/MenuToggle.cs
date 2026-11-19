using UnityEngine;
using UnityEngine.InputSystem;

//opens and closes a menu with a button press, from anywhere in the scene
//the menu appears in front of the player and stays put until it is closed
public class MenuToggle : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Button that opens and closes the menu.")]
    [SerializeField] InputAction toggleAction = DefaultToggleAction();

    [Header("Menu")]
    [Tooltip("The menu object to show and hide.")]
    [SerializeField] GameObject menu;
    [Tooltip("The player's head. The menu opens in front of wherever this is looking.")]
    [SerializeField] Transform head;
    [Tooltip("How far in front of the player the menu opens, in meters.")]
    [SerializeField, Min(0.1f)] float distance = 1.5f;
    [Tooltip("How far above (or below, if negative) eye level the menu sits, in meters.")]
    [SerializeField] float heightOffset = -0.1f;

    [Header("While The Menu Is Open")]
    [Tooltip("Objects that only exist while the menu is open, like the pointer ray.")]
    [SerializeField] GameObject[] showWhileOpen;
    [Tooltip("Objects hidden while the menu is open, like the wand orb.")]
    [SerializeField] GameObject[] hideWhileOpen;
    [Tooltip("Scripts turned off while the menu is open, like sigil drawing and casting.")]
    [SerializeField] Behaviour[] disableWhileOpen;

    public bool IsOpen { get; private set; }

    static InputAction DefaultToggleAction()
    {
        //the menu button on the left controller, or M on a keyboard
        InputAction action = new InputAction("Toggle Menu", InputActionType.Button, "<XRController>{LeftHand}/{MenuButton}");
        action.AddBinding("<Keyboard>/m");
        return action;
    }

    void OnEnable()
    {
        toggleAction.performed += OnToggle;
        toggleAction.Enable();
    }
    void OnDisable()
    {
        toggleAction.performed -= OnToggle;
        toggleAction.Disable();
    }
    void Start()
    {
        //always begin closed
        IsOpen = false;
        ApplyState();
    }
    void OnToggle(InputAction.CallbackContext context)
    {
        Toggle();
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }
    public void Open()
    {
        PlaceInFrontOfPlayer();
        IsOpen = true;
        ApplyState();
    }
    //also called by the menu's Resume button
    public void Close()
    {
        IsOpen = false;
        ApplyState();
    }

    void PlaceInFrontOfPlayer()
    {
        if (menu == null || head == null) return;

        //ignore head tilt so the menu always stands upright
        Vector3 forward = head.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = head.up;
        forward.Normalize();

        Vector3 position = head.position + forward * distance;
        position.y += heightOffset;

        //a canvas reads correctly when its forward points away from the viewer
        menu.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
    }
    void ApplyState()
    {
        if (menu != null) menu.SetActive(IsOpen);

        foreach (GameObject item in showWhileOpen)
            if (item != null) item.SetActive(IsOpen);
        foreach (GameObject item in hideWhileOpen)
            if (item != null) item.SetActive(!IsOpen);
        foreach (Behaviour item in disableWhileOpen)
            if (item != null) item.enabled = !IsOpen;
    }
}
