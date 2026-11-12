using UnityEngine;

public class MainMenuAudio : MonoBehaviour
{
    public string musicClip, sfxClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.instance.PlayMusic(musicClip);
    }

    public void ButtonAudio()
    {
        AudioManager.instance.PlaySFX(sfxClip);
    }
}
