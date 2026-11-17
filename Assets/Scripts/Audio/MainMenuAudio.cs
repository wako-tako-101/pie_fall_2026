using UnityEngine;

public class MainMenuAudio : MonoBehaviour
{
    public string musicClip, sfxClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //no manager when a scene is played on its own, and menus without music leave the clip empty
        if(AudioManager.instance != null && !string.IsNullOrEmpty(musicClip)) AudioManager.instance.PlayMusic(musicClip);
    }

    public void ButtonAudio()
    {
        if(AudioManager.instance != null) AudioManager.instance.PlaySFX(sfxClip);
    }
}
