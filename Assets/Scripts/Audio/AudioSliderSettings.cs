using UnityEngine;
using UnityEngine.UI;

public class AudioSliderSettings : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.ConnectSlider(musicSlider);
            AudioManager.instance.ConnectSlider(sfxSlider);
        }
    }

    private void OnDestroy()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.ConnectSlider(null);
            AudioManager.instance.ConnectSlider(null);
        }
    }
}
