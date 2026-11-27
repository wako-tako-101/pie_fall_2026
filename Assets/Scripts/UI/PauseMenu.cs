using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pauseMenuGUI;
    [SerializeField] private CanvasGroup pauseMenuCanvasGroup;
    [SerializeField] private CanvasGroup pausePanel;
    [SerializeField] private CanvasGroup settingsMenu;

    [Header("Settings")]
    [Tooltip("The button that will open the pause menu.")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private static bool isPaused = false;

    //for going back to the main menu
    // [Header("Main Menu Settings")]
    // [SerializeField] private string mainMenuSceneName = "TitleScreen";
    // [SerializeField] private int sceneToLoad = 0;
    // [SerializeField] private bool usingSceneInt = false;


    //initialize pause menu but keep hidden
    void Awake()
    {
        // if(pauseAction != null && pauseMenuCanvasGroup != null)
        // {
        //     pauseMenuGUI.SetActive(false);
        // }

        isPaused = false;
        Time.timeScale = 1f;
        SetVisibility(pauseMenuCanvasGroup, false);
        SetVisibility(pausePanel, false);
        SetVisibility(settingsMenu, false);
    }

    //enable pause action when enabled
    private void OnEnable()
    {
        if(pauseAction != null)
        {
            pauseAction.action.Enable();
        }
    }

    //disable pause action when disabled
    private void OnDisable()
    {
        if(pauseAction != null)
        {
            pauseAction.action.Disable();
            Time.timeScale = 1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pauseAction != null && pauseAction.action.WasPressedThisFrame())
        {
            TogglePause();
            Debug.Log("Pause action pressed!");
        }
    }

    //toggle pause functionality from the game
    public void TogglePause()
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0f : 1f;
        SetVisibility(pauseMenuCanvasGroup, isPaused);
        SetVisibility(pausePanel, isPaused);
        SetVisibility(settingsMenu, false);
        
        // if(pauseMenuGUI != null)
        // {
        //     pauseMenuGUI.SetActive(isPaused);
        // }

        // if (isPaused)
        // {
        //     ToggleMenu(true);
        //     Time.timeScale = 0f;
        //     Cursor.lockState = CursorLockMode.None;
        //     Cursor.visible = true;
        // }
        // else
        // {
        //     ToggleMenu(false);
        //     Time.timeScale = 1f;
        //     Cursor.lockState = CursorLockMode.Locked;
        //     Cursor.visible = false;
        // }

        Debug.Log("Game Paused: " + isPaused);
    }

    //toggle resume functionality from the pause menu
    public void ToggleResume()
    {
        if (isPaused)
        {
            TogglePause();
            Debug.Log("Game resumed.");
        }
    }

    //going to the settings menu from the pause menu
    public void ToggleSettingsMenu()
    {
        if(!isPaused) return;

        SetVisibility(pauseMenuCanvasGroup, true);
        SetVisibility(pausePanel, false);
        SetVisibility(settingsMenu, true);
        Debug.Log("Settings menu opened.");
    }

    //returning to the pause menu from the settings menu
    public void BackToPauseMenu()
    {
        if(!isPaused) return;

        SetVisibility(pauseMenuCanvasGroup, true);        
        SetVisibility(settingsMenu, false);
        SetVisibility(pausePanel, true);
        Debug.Log("Back to pause menu.");
    }

    public void SetVisibility(CanvasGroup group, bool isVisible)
    {
        if(group == null) return;
        
        group.alpha = isVisible ? 1f : 0f;
        group.interactable = isVisible;
        group.blocksRaycasts = isVisible;
    }

    //for going back to the main menu
    // public void LoadMainMenu()
    // {
    //     Time.timeScale = 1f; // Reset time scale before loading the main menu

    //     if (usingSceneInt)
    //     {
    //         SceneManager.LoadScene(sceneToLoad);
    //     }
    //     else
    //     {
    //         SceneManager.LoadScene(mainMenuSceneName);
    //     }
        
    // }
}
