using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct AudioData
{
    public string name;
    public AudioClip clip;
}


public class AudioManager : MonoBehaviour
{
    [Header("Audio Manager Instance")]
    [Tooltip("Attach this script to the Audio Manager")]
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;


    [Header("Volume Settings")]
    //[Range(0f, 1f)]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private float musicVolume = 0.5f;

    //[Range(0f, 1f)]
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private float sfxVolume = 0.5f;

    //[Range(0f, 1f)]
    [SerializeField] private Slider allVolumeSlider;
    [SerializeField] private float allVolume = 0.5f;

    [Header("Playback Settings")]
    [SerializeField] private bool isMuted = false;
    [SerializeField] private bool isLooping = false;


    [Header("Audio Library")]
    [Tooltip("List of available audio clips for music")]
    [SerializeField] private List<AudioData> musicLibrary = new List<AudioData>();

    [Tooltip("List of available audio clips for sound effects")]
    [SerializeField] private List<AudioData> sfxLibrary = new List<AudioData>();

    [Tooltip("Dictionary of available audio clips")]
    private Dictionary<string, AudioClip> musicDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();



    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        //to load clips into the dictinary
        foreach(var entry in musicLibrary)
        {
            if(string.IsNullOrWhiteSpace(entry.name) || entry.clip == null) continue;

            if (musicDictionary.ContainsKey(entry.name))
            {
                Debug.LogWarning("Duplicate music clip name: " + entry.name);
                continue;
            }

            musicDictionary[entry.name] = entry.clip;
        }

        foreach(var entry in sfxLibrary)
        {
            if(string.IsNullOrWhiteSpace(entry.name) || entry.clip == null) continue;

            if (sfxDictionary.ContainsKey(entry.name))
            {
                Debug.LogWarning("Duplicate sfx clip name: " + entry.name);
                continue;
            }

            sfxDictionary[entry.name] = entry.clip;
        }

        if(musicSource == null) musicSource = GetComponent<AudioSource>();
        if(sfxSource == null) sfxSource = GetComponent<AudioSource>();

        musicSource.playOnAwake = true;
        sfxSource.playOnAwake = false;

        musicSource.loop = isLooping;
        sfxSource.loop = isLooping;
    }

    //MUSIC
    public void PlayMusic(string clipName)
    {
        if(musicDictionary.ContainsKey(clipName))
        {
            if (musicSource.isPlaying)
            {
                Debug.Log("Music is already playing.");
                return;
            }

            musicSource.Stop();
            musicSource.clip = musicDictionary[clipName];
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Music clip not found: " + clipName);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    //SFX
    public void PlaySFX(string clipName)
    {
        if(sfxDictionary.ContainsKey(clipName))
        {
            if (sfxSource.isPlaying)
            {
                Debug.Log("SFX is already playing.");
                return;
            }

            sfxSource.PlayOneShot(sfxDictionary[clipName]);
        }
        else
        {
            Debug.LogWarning("SFX clip not found: " + clipName);
        }
    }

    //VOLUME
    //the three volumes live in the fields above, the sliders only edit them
    private void ApplyVolume()
    {
        musicSource.volume = musicVolume * allVolume;
        sfxSource.volume = sfxVolume * allVolume;

        musicSource.mute = isMuted;
        sfxSource.mute = isMuted;
    }

    //reads whichever sliders are connected, then applies the result
    public void UpdateVolume()
    {
        if(musicVolumeSlider != null) musicVolume = musicVolumeSlider.value;
        if(sfxVolumeSlider != null) sfxVolume = sfxVolumeSlider.value;
        if(allVolumeSlider != null) allVolume = allVolumeSlider.value;

        ApplyVolume();
    }

    public void SetVolume(float musicVolume, float sfxVolume, float allVolume)
    {
        this.musicVolume = Mathf.Clamp01(musicVolume);
        this.sfxVolume = Mathf.Clamp01(sfxVolume);
        this.allVolume = Mathf.Clamp01(allVolume);

        ApplyVolume();
        SyncSliders();
    }

    //each slider only ever changes its own volume
    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyVolume();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolume();
    }

    public void SetAllVolume(float value)
    {
        allVolume = Mathf.Clamp01(value);
        ApplyVolume();
    }

    //Mute/Unmute
    public void SetMute(bool isMuted)
    {
        this.isMuted = isMuted;
        ApplyVolume();
    }

    //Slider Logic
    //hooks up the sliders of whichever menu is on screen, pass null for any that are missing
    public void ConnectSliders(Slider music, Slider sfx, Slider all)
    {
        DisconnectSliders();

        musicVolumeSlider = music;
        sfxVolumeSlider = sfx;
        allVolumeSlider = all;

        SetupSlider(musicVolumeSlider, musicVolume, SetMusicVolume);
        SetupSlider(sfxVolumeSlider, sfxVolume, SetSfxVolume);
        SetupSlider(allVolumeSlider, allVolume, SetAllVolume);
    }

    public void DisconnectSliders()
    {
        if(musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if(sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
        if(allVolumeSlider != null) allVolumeSlider.onValueChanged.RemoveListener(SetAllVolume);

        musicVolumeSlider = null;
        sfxVolumeSlider = null;
        allVolumeSlider = null;
    }

    private void SetupSlider(Slider slider, float value, UnityEngine.Events.UnityAction<float> onChanged)
    {
        if(slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        //show the saved volume without firing the listener
        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(onChanged);
    }

    private void SyncSliders()
    {
        if(musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(musicVolume);
        if(sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
        if(allVolumeSlider != null) allVolumeSlider.SetValueWithoutNotify(allVolume);
    }

    private void OnDestroy()
    {
        //a duplicate manager never became the instance, so it has nothing to unhook
        if(instance != this) return;

        DisconnectSliders();
        instance = null;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicSource.loop = isLooping;
        sfxSource.loop = isLooping;

        //the saved volumes are the source of truth, not wherever the sliders happen to sit
        ConnectSliders(musicVolumeSlider, sfxVolumeSlider, allVolumeSlider);
        ApplyVolume();
    }
}
