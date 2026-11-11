using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Quitting the game...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif

            Application.Quit();
    }
}