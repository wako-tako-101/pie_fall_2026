using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pauseMenuGUI;

    [Header("Settings")]
    [Tooltip("The button that will open the pause menu.")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private static bool isPaused = false;

    //for going back to the main menu
    // [Header("Main Menu Settings")]
    // [SerializeField] private string mainMenuSceneName = "TitleScreen";
    // [SerializeField] private int sceneToLoad = 0;
    // [SerializeField] private bool usingSceneInt = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(pauseAction != null)
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
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
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
