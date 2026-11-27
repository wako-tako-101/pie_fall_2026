using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pauseMenuGUI;
    [SerializeField] private CanvasGroup pauseMenuCanvasGroup;

    [Header("Settings")]
    [Tooltip("The button that will open the pause menu.")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private static bool isPaused = false;

    //for going back to the main menu
    // [Header("Main Menu Settings")]
    // [SerializeField] private string mainMenuSceneName = "TitleScreen";
    // [SerializeField] private int sceneToLoad = 0;
    // [SerializeField] private bool usingSceneInt = false;


    void Awake()
    {
        if(pauseAction != null && pauseMenuCanvasGroup != null)
        {
            pauseMenuGUI.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if(pauseAction != null)
        {
            pauseAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if(pauseAction != null)
        {
            pauseAction.action.Disable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pauseAction != null && pauseAction.action.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if(pauseMenuGUI != null)
        {
            pauseMenuGUI.SetActive(isPaused);
        }

        if (isPaused)
        {
            ToggleMenu(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            ToggleMenu(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log("Game Paused: " + isPaused);
    }

    public void ToggleResume()
    {
        if (isPaused)
        {
            TogglePause();
            Debug.Log("Game resumed.");
        }
    }

    public void ToggleMenu(bool isVisible)
    {
        if(pauseMenuGUI == null || pauseMenuCanvasGroup == null) return;

        if(isVisible)
        {            
            pauseMenuGUI.SetActive(isVisible);
            pauseMenuCanvasGroup.alpha = 1f;
            pauseMenuCanvasGroup.interactable = isVisible;
            pauseMenuCanvasGroup.blocksRaycasts = isVisible;
        }
        else
        {
            pauseMenuGUI.SetActive(isVisible);
            pauseMenuCanvasGroup.alpha = 0f;
            pauseMenuCanvasGroup.interactable = isVisible;
            pauseMenuCanvasGroup.blocksRaycasts = isVisible;
        }
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
