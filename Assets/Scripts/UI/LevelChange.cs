using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour
{
    //[SerializeField] int levelInt;
    [Tooltip("Please ensure the name is spelled EXACTLY")]
    [Header("Scene to switch to")]
    [SerializeField] string levelName;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("Invalid level or level name.");
        }
    }

    public void LevelSwitch()
    {
        //if levelName is valid
        if(!string.IsNullOrEmpty(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
    }
}
