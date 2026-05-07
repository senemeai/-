using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音频源")]
    public AudioSource audioSourceBGM;
    public AudioSource audioSourceWave;

    [Header("音频剪辑")]
    public AudioClip bgmClip;
    public AudioClip waveClip;

    private const string KEY_BGM_VOLUME = "BGM_Volume";
    private const string KEY_WAVE_VOLUME = "Wave_Volume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化音频源
        InitAudioSources();
        // 加载保存的音量
        LoadVolumes();
    }

    private void InitAudioSources()
    {
        if (audioSourceBGM == null)
        {
            audioSourceBGM = gameObject.AddComponent<AudioSource>();
        }
        if (audioSourceWave == null)
        {
            audioSourceWave = gameObject.AddComponent<AudioSource>();
        }

        audioSourceBGM.clip = bgmClip;
        audioSourceBGM.loop = true;
        audioSourceBGM.playOnAwake = true;

        audioSourceWave.clip = waveClip;
        audioSourceWave.loop = true;
        audioSourceWave.playOnAwake = true;

        // 开始播放
        if (bgmClip != null) audioSourceBGM.Play();
        if (waveClip != null) audioSourceWave.Play();
    }

    private void LoadVolumes()
    {
        float bgmVol = PlayerPrefs.GetFloat(KEY_BGM_VOLUME, 0.8f);
        float waveVol = PlayerPrefs.GetFloat(KEY_WAVE_VOLUME, 0.5f);

        SetBGMVolume(bgmVol);
        SetWaveVolume(waveVol);
    }

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (audioSourceBGM != null)
        {
            audioSourceBGM.volume = volume;
        }
        PlayerPrefs.SetFloat(KEY_BGM_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public void SetWaveVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (audioSourceWave != null)
        {
            audioSourceWave.volume = volume;
        }
        PlayerPrefs.SetFloat(KEY_WAVE_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(KEY_BGM_VOLUME, 0.8f);
    }

    public float GetWaveVolume()
    {
        return PlayerPrefs.GetFloat(KEY_WAVE_VOLUME, 0.5f);
    }
}