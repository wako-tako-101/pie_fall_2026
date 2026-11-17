using UnityEngine;
using UnityEngine.UI;

//put this on any menu that has volume sliders
//it hands that menu's sliders to the AudioManager, which survives between scenes
public class AudioSliderSettings : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider masterSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.ConnectSliders(musicSlider, sfxSlider, masterSlider);
        }
    }

    private void OnDestroy()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.DisconnectSliders();
        }
    }
}
