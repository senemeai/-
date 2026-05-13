using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicSelectorUI : MonoBehaviour
{
    [Header("入口按钮")]
    public Button btnMusicPlayer;

    [Header("面板")]
    public GameObject panelMusicSelector;
    public Button btnClose;
    public Button btnUpload;

    [Header("下拉框")]
    public TMP_Dropdown dropdownBGM;
    public TMP_Dropdown dropdownWave;

    [Header("上传面板")]
    public MusicUploadUI uploadUI;

    private List<CustomAudioEntry> userAudios = new List<CustomAudioEntry>();

    void Start()
    {
        btnMusicPlayer.onClick.AddListener(OpenPanel);
        btnClose.onClick.AddListener(ClosePanel);
        btnUpload.onClick.AddListener(OpenUploadPanel);
        dropdownBGM.onValueChanged.AddListener(OnBGMChanged);
        dropdownWave.onValueChanged.AddListener(OnWaveChanged);

        panelMusicSelector.SetActive(false);

        if (uploadUI != null)
            uploadUI.OnUploadComplete += RefreshDropdowns;
    }

    public void OpenPanel()
    {
        panelMusicSelector.SetActive(true);
        RefreshDropdowns();
    }

    void ClosePanel()
    {
        panelMusicSelector.SetActive(false);
    }

    void OpenUploadPanel()
    {
        uploadUI?.OpenPanel();
    }

    public void RefreshDropdowns()
    {
        var player = UserDataManager.Instance.GetPlayer(UserDataManager.Instance.CurrentLoggedInUser);
        userAudios = player?.customAudios ?? new List<CustomAudioEntry>();

        var bgmList = userAudios.Where(a => a.audioType == "BGM").ToList();
        dropdownBGM.ClearOptions();
        var bgmOptions = new List<string> { "默认背景音乐" };
        bgmOptions.AddRange(bgmList.Select(a => a.displayName));
        dropdownBGM.AddOptions(bgmOptions);

        var waveList = userAudios.Where(a => a.audioType == "Wave").ToList();
        dropdownWave.ClearOptions();
        var waveOptions = new List<string> { "默认环境音" };
        waveOptions.AddRange(waveList.Select(a => a.displayName));
        dropdownWave.AddOptions(waveOptions);
    }

    void OnBGMChanged(int index)
    {
        if (index == 0)
        {
            AudioManager.Instance.RestoreDefaultBGM();
            return;
        }
        var list = userAudios.Where(a => a.audioType == "BGM").ToList();
        if (index - 1 < list.Count)
            AudioManager.Instance.PlayCustomBGM(list[index - 1].fileName);
    }

    void OnWaveChanged(int index)
    {
        if (index == 0)
        {
            AudioManager.Instance.RestoreDefaultWave();
            return;
        }
        var list = userAudios.Where(a => a.audioType == "Wave").ToList();
        if (index - 1 < list.Count)
            AudioManager.Instance.PlayCustomWave(list[index - 1].fileName);
    }

    void OnDestroy()
    {
        btnMusicPlayer.onClick.RemoveAllListeners();
        btnClose.onClick.RemoveAllListeners();
        btnUpload.onClick.RemoveAllListeners();
        dropdownBGM.onValueChanged.RemoveAllListeners();
        dropdownWave.onValueChanged.RemoveAllListeners();
        if (uploadUI != null)
            uploadUI.OnUploadComplete -= RefreshDropdowns;
    }
}