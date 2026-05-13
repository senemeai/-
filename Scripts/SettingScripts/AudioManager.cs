using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("“Ù∆µ‘¥")]
    public AudioSource audioSourceBGM;
    public AudioSource audioSourceWave;

    [Header("ƒ¨»œ“Ù∆µ")]
    public AudioClip bgmClip;
    public AudioClip waveClip;

    private AudioClip defaultBGM;
    private AudioClip defaultWave;

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

        InitAudioSources();
        LoadVolumes();
    }

    private void InitAudioSources()
    {
        if (audioSourceBGM == null)
            audioSourceBGM = gameObject.AddComponent<AudioSource>();
        if (audioSourceWave == null)
            audioSourceWave = gameObject.AddComponent<AudioSource>();

        defaultBGM = bgmClip;
        defaultWave = waveClip;

        audioSourceBGM.clip = bgmClip;
        audioSourceBGM.loop = true;
        audioSourceBGM.playOnAwake = true;

        audioSourceWave.clip = waveClip;
        audioSourceWave.loop = true;
        audioSourceWave.playOnAwake = true;

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
        if (audioSourceBGM != null) audioSourceBGM.volume = volume;
        PlayerPrefs.SetFloat(KEY_BGM_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public void SetWaveVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (audioSourceWave != null) audioSourceWave.volume = volume;
        PlayerPrefs.SetFloat(KEY_WAVE_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetBGMVolume() => PlayerPrefs.GetFloat(KEY_BGM_VOLUME, 0.8f);
    public float GetWaveVolume() => PlayerPrefs.GetFloat(KEY_WAVE_VOLUME, 0.5f);

    // ========== ◊‘∂®“Â“Ù∆µ«–ªª ==========
    public void ResumeDefaultAudio()
    {
        if (audioSourceBGM != null)
        {
            audioSourceBGM.clip = defaultBGM;
            if (defaultBGM != null && !audioSourceBGM.isPlaying)
                audioSourceBGM.Play();
        }
        if (audioSourceWave != null)
        {
            audioSourceWave.clip = defaultWave;
            if (defaultWave != null && !audioSourceWave.isPlaying)
                audioSourceWave.Play();
        }
    }
    public void StopAllAudio()
    {
        if (audioSourceBGM != null) audioSourceBGM.Stop();
        if (audioSourceWave != null) audioSourceWave.Stop();
    }
    public void RestoreDefaultBGM()
    {
        audioSourceBGM.clip = defaultBGM;
        if (defaultBGM != null) audioSourceBGM.Play();
    }

    public void RestoreDefaultWave()
    {
        audioSourceWave.clip = defaultWave;
        if (defaultWave != null) audioSourceWave.Play();
    }

    public void PlayCustomBGM(string fileName) => StartCoroutine(LoadAndPlay(fileName, audioSourceBGM));
    public void PlayCustomWave(string fileName) => StartCoroutine(LoadAndPlay(fileName, audioSourceWave));

    private IEnumerator LoadAndPlay(string fileName, AudioSource targetSource)
    {
        string user = UserDataManager.Instance.CurrentLoggedInUser;
        string path = Path.Combine(Application.persistentDataPath, "CustomMusic", user, fileName);

        if (!File.Exists(path))
        {
            Debug.LogError("“Ù∆µŒƒº˛≤ª¥Ê‘⁄: " + path);
            yield break;
        }

        AudioType type = GetAudioType(Path.GetExtension(fileName));
        string url = "file://" + path;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                targetSource.clip = clip;
                targetSource.Play();
            }
            else
            {
                Debug.LogError("º”‘ÿ“Ù∆µ ß∞‹: " + www.error);
            }
        }
    }

    private AudioType GetAudioType(string ext)
    {
        switch (ext.ToLower())
        {
            case ".wav": return AudioType.WAV;
            case ".ogg": return AudioType.OGGVORBIS;
            default: return AudioType.MPEG;
        }
    }
}