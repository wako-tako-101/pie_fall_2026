using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour
{
    //[SerializeField] int levelInt;
    [Tooltip("Please ensure the name is spelled EXACTLY")]
    [Header("Scene to switch to (Use either level name or scene index)")]
    [SerializeField] string levelName;
    [SerializeField] int levelIndex;

    public bool usingLevelIndex = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("Invalid level or level name.");
        }

        if(usingLevelIndex && levelIndex < 0)
        {
            Debug.LogError("Invalid level index.");
        }
    }

    public void LevelSwitch()
    {
        //if levelName is valid
        if(usingLevelIndex)
        {
            SceneManager.LoadScene(levelIndex, LoadSceneMode.Single);
        }
        else if(!string.IsNullOrEmpty(levelName) && !usingLevelIndex)
        {
            SceneManager.LoadScene(levelName, LoadSceneMode.Single);
        }
    }
}
