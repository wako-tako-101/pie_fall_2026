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
    [Range(0f, 1f)]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private float musicVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private float sfxVolume = 0.5f;

    [Range(0f, 1f)]
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
    public void UpdateVolume()
    {
        musicSource.volume = musicVolumeSlider.value * allVolumeSlider.value;
        sfxSource.volume = sfxVolumeSlider.value * allVolumeSlider.value;

        musicSource.mute = isMuted;
        sfxSource.mute = isMuted;
        
        musicVolume = musicVolumeSlider.value;
        sfxVolume = sfxVolumeSlider.value;
        allVolume = allVolumeSlider.value;
    }

    public void SetVolume(float musicVolume, float sfxVolume, float allVolume)
    {
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        allVolume = Mathf.Clamp01(allVolume);

        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;
        allVolumeSlider.value = allVolume;

        UpdateVolume();
        if(musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(musicVolume);
        if(sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
        if(allVolumeSlider != null) allVolumeSlider.SetValueWithoutNotify(allVolume);

    }

    //Mute/Unmute
    public void SetMute(bool isMuted)
    {
        this.isMuted = isMuted;
        UpdateVolume();
    }

    //Slider Logic
    public void ConnectSlider(Slider slider)
    {
        if(musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener((value) => SetVolume(value, sfxVolume, allVolume));
        musicVolumeSlider = slider;

        if(musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.wholeNumbers = false;

            musicVolumeSlider.SetValueWithoutNotify(musicVolume);
            musicVolumeSlider.onValueChanged.AddListener((value) => SetVolume(value, sfxVolume, allVolume));
        }

        if(sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener((value) => SetVolume(musicVolume, value, allVolume));
        sfxVolumeSlider = slider;
        if(sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.wholeNumbers = false;

            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
            sfxVolumeSlider.onValueChanged.AddListener((value) => SetVolume(musicVolume, value, allVolume));
        }

        if(allVolumeSlider != null) allVolumeSlider.onValueChanged.RemoveListener((value) => SetVolume(musicVolume, sfxVolume, value));
        allVolumeSlider = slider;
        if(allVolumeSlider != null)
        {
            allVolumeSlider.minValue = 0f;
            allVolumeSlider.maxValue = 1f;
            allVolumeSlider.wholeNumbers = false;

            allVolumeSlider.SetValueWithoutNotify(allVolume);
            allVolumeSlider.onValueChanged.AddListener((value) => SetVolume(musicVolume, sfxVolume, value));
        }
    }

    private void OnDestroy()
    {
        if(musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener((value) => SetVolume(value, sfxVolume, allVolume));
        if(sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener((value) => SetVolume(musicVolume, value, allVolume));
        if(allVolumeSlider != null) allVolumeSlider.onValueChanged.RemoveListener((value) => SetVolume(musicVolume, sfxVolume, value));
        if(instance == this) instance = null;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicSource.volume = musicVolumeSlider.value * allVolumeSlider.value;
        sfxSource.volume = sfxVolumeSlider.value * allVolumeSlider.value;
        musicSource.loop = isLooping;
        musicSource.mute = isMuted;
        sfxSource.loop = isLooping;
        sfxSource.mute = isMuted;
    }
}
