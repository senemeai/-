using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicUploadUI : MonoBehaviour
{
    public event Action OnUploadComplete;

    [Header("面板")]
    public GameObject panelUpload;
    public Button btnClose;
    public Button btnCancel;
    public Button btnConfirm;
    public Button btnSelectFile;

    [Header("输入")]
    public TMP_InputField inputName;
    public Toggle toggleBGM;
    public Toggle toggleWave;
    public TextMeshProUGUI textPath;
    public TextMeshProUGUI textError;

    private string selectedSourcePath = "";
    private string selectedFileName = "";

    void Start()
    {
        btnClose.onClick.AddListener(ClosePanel);
        btnCancel.onClick.AddListener(ClosePanel);
        btnConfirm.onClick.AddListener(OnConfirm);
        btnSelectFile.onClick.AddListener(OnSelectFile);
        panelUpload.SetActive(false);
    }

    public void OpenPanel()
    {
        panelUpload.SetActive(true);
        ResetForm();
    }

    void ResetForm()
    {
        inputName.text = "";
        toggleBGM.isOn = false;
        toggleWave.isOn = false;
        textPath.text = "未选择文件";
        textPath.color = new Color(0.6f, 0.6f, 0.6f);
        textError.gameObject.SetActive(false);
        selectedSourcePath = "";
        selectedFileName = "";
    }

    void OnSelectFile()
    {
        var extensions = new[] { new SFB.ExtensionFilter("音频文件", "mp3", "wav", "ogg") };
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel(
            "选择要上传的音乐文件",
            "",
            extensions,
            false
        );

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedSourcePath = paths[0];
            selectedFileName = Path.GetFileName(paths[0]);
            textPath.text = selectedFileName;
            textPath.color = Color.white;
            textError.gameObject.SetActive(false);
        }
    }

    void OnConfirm()
    {
        // 新增：必须选一个类型
        if (!toggleBGM.isOn && !toggleWave.isOn)
        {
            ShowError("请选择音乐类型：背景音乐 或 环境音效");
            return;
        }

        string displayName = inputName.text.Trim();
        if (string.IsNullOrEmpty(displayName))
        {
            ShowError("请输入音乐名称");
            return;
        }

        if (string.IsNullOrEmpty(selectedSourcePath))
        {
            ShowError("请选择本地音乐文件");
            return;
        }

        string ext = Path.GetExtension(selectedSourcePath).ToLower();
        if (ext != ".mp3" && ext != ".wav" && ext != ".ogg")
        {
            ShowError("仅支持 mp3、wav、ogg 格式");
            return;
        }

        string audioType = toggleBGM.isOn ? "BGM" : "Wave";
        string uniqueName = $"{audioType}_{DateTime.Now.Ticks}{ext}";

        string userName = UserDataManager.Instance.CurrentLoggedInUser;
        if (string.IsNullOrEmpty(userName))
        {
            ShowError("当前未登录，无法上传");
            return;
        }

        string destDir = Path.Combine(Application.persistentDataPath, "CustomMusic", userName);
        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        string destPath = Path.Combine(destDir, uniqueName);
        try
        {
            File.Copy(selectedSourcePath, destPath);
        }
        catch (Exception e)
        {
            ShowError("文件复制失败: " + e.Message);
            return;
        }

        CustomAudioEntry entry = new CustomAudioEntry
        {
            displayName = displayName,
            fileName = uniqueName,
            audioType = audioType
        };

        UserDataManager.Instance.AddCustomAudio(userName, entry);

        OnUploadComplete?.Invoke();
        ClosePanel();
    }

    void ShowError(string msg)
    {
        textError.text = msg;
        textError.gameObject.SetActive(true);
    }

    void ClosePanel()
    {
        panelUpload.SetActive(false);
    }

    void OnDestroy()
    {
        btnClose.onClick.RemoveAllListeners();
        btnCancel.onClick.RemoveAllListeners();
        btnConfirm.onClick.RemoveAllListeners();
        btnSelectFile.onClick.RemoveAllListeners();
    }
}