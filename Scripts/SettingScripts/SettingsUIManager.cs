using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingsUIManager : MonoBehaviour
{
    [Header("面板")]
    public GameObject panelSettings;
    public GameObject panelEditInfo;
    public GameObject panelExitConfirm;

    [Header("音频设置")]
    public Slider sliderBGM;
    public TextMeshProUGUI textBGMValue;
    public Slider sliderWave;
    public TextMeshProUGUI textWaveValue;

    [Header("用户信息展示")]
    public TextMeshProUGUI textInfoUsername;
    public TextMeshProUGUI textInfoGender;
    public TextMeshProUGUI textInfoAge;
    public Button btnEditInfo;

    [Header("修改信息")]
    public TMP_InputField inputNewUsername;
    public TMP_Dropdown dropdownGender;
    public TMP_InputField inputNewAge;
    public TMP_InputField inputNewPassword;
    public TMP_InputField inputConfirmPassword;
    public TextMeshProUGUI textEditTips;
    public Button btnSaveEdit;
    public Button btnCancelEdit;

    [Header("其他按钮")]
    public Button btnSettings;      // 打开设置的按钮（GameUI里的）
    public Button btnCloseSettings; // 设置面板右上角关闭
    public Button btnExitGame;      // 退出游戏
    public Button btnConfirmExit;   // 确认退出
    public Button btnCancelExit;    // 取消退出

    private string originalUsername = "";
    private string originalGender = "";
    private int originalAge = 0;

    private void Start()
    {
        // 绑定事件
        btnSettings.onClick.AddListener(OpenSettings);
        btnCloseSettings.onClick.AddListener(CloseSettings);
        btnExitGame.onClick.AddListener(OnExitGameClick);
        btnConfirmExit.onClick.AddListener(OnConfirmExit);
        btnCancelExit.onClick.AddListener(OnCancelExit);
        btnEditInfo.onClick.AddListener(OpenEditPanel);
        btnSaveEdit.onClick.AddListener(OnSaveEdit);
        btnCancelEdit.onClick.AddListener(CloseEditPanel);

        // 绑定Slider事件
        sliderBGM.onValueChanged.AddListener(OnBGMVolumeChanged);
        sliderWave.onValueChanged.AddListener(OnWaveVolumeChanged);

        // 初始化性别下拉框
        dropdownGender.ClearOptions();
        dropdownGender.AddOptions(new System.Collections.Generic.List<string> { "男", "女" });

        // 初始隐藏
        panelSettings.SetActive(false);
        panelEditInfo.SetActive(false);
        panelExitConfirm.SetActive(false);
    }

    // 打开设置面板
    public void OpenSettings()
    {
        panelSettings.SetActive(true);

        // 加载音量
        sliderBGM.value = AudioManager.Instance.GetBGMVolume();
        sliderWave.value = AudioManager.Instance.GetWaveVolume();
        UpdateVolumeText(sliderBGM.value, textBGMValue);
        UpdateVolumeText(sliderWave.value, textWaveValue);

        // 加载用户信息
        LoadUserInfo();
    }

    // 关闭设置面板
    public void CloseSettings()
    {
        panelSettings.SetActive(false);
        panelEditInfo.SetActive(false);
        panelExitConfirm.SetActive(false);
    }

    // 音量变化回调
    private void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance.SetBGMVolume(value);
        UpdateVolumeText(value, textBGMValue);
    }

    private void OnWaveVolumeChanged(float value)
    {
        AudioManager.Instance.SetWaveVolume(value);
        UpdateVolumeText(value, textWaveValue);
    }

    private void UpdateVolumeText(float value, TextMeshProUGUI text)
    {
        text.text = Mathf.RoundToInt(value * 100) + "%";
    }

    // 加载并显示用户信息
    private void LoadUserInfo()
    {
        string currentUser = UserDataManager.Instance.CurrentLoggedInUser;
        if (string.IsNullOrEmpty(currentUser))
        {
            textInfoUsername.text = "用户名：未登录";
            textInfoGender.text = "性别：-";
            textInfoAge.text = "年龄：-";
            btnEditInfo.interactable = false;
            return;
        }

        PlayerData player = UserDataManager.Instance.GetPlayer(currentUser);
        if (player != null)
        {
            textInfoUsername.text = "用户名：" + player.username;
            textInfoGender.text = "性别：" + player.gender;
            textInfoAge.text = "年龄：" + player.age;

            // 记录原始值，用于取消时恢复
            originalUsername = player.username;
            originalGender = player.gender;
            originalAge = player.age;

            btnEditInfo.interactable = true;
        }
    }

    // 打开修改信息面板
    private void OpenEditPanel()
    {
        // 预填充当前数据
        inputNewUsername.text = originalUsername;
        dropdownGender.value = originalGender == "女" ? 1 : 0;
        inputNewAge.text = originalAge.ToString();
        inputNewPassword.text = "";
        inputConfirmPassword.text = "";
        textEditTips.gameObject.SetActive(false);

        panelEditInfo.SetActive(true);
    }

    // 关闭修改信息面板
    private void CloseEditPanel()
    {
        panelEditInfo.SetActive(false);
        textEditTips.gameObject.SetActive(false);
    }

    // 保存修改
    private void OnSaveEdit()
    {
        string newUsername = inputNewUsername.text.Trim();
        string gender = dropdownGender.options[dropdownGender.value].text;
        string ageStr = inputNewAge.text;
        string newPassword = inputNewPassword.text;
        string confirmPassword = inputConfirmPassword.text;

        // 验证
        if (string.IsNullOrEmpty(newUsername))
        {
            ShowEditTips("用户名不能为空", false);
            return;
        }

        // 年龄验证
        if (!int.TryParse(ageStr, out int newAge) || newAge <= 0 || newAge >= 150)
        {
            ShowEditTips("请输入有效的年龄", false);
            return;
        }

        // 密码验证（如果填了密码才验证）
        if (!string.IsNullOrEmpty(newPassword))
        {
            if (newPassword.Length < 6)
            {
                ShowEditTips("密码不能少于6位", false);
                return;
            }
            if (newPassword != confirmPassword)
            {
                ShowEditTips("两次密码不一致", false);
                return;
            }
        }

        // 执行更新
        bool success = UserDataManager.Instance.UpdatePlayerInfo(
            originalUsername,
            newUsername,
            gender,
            newAge,
            newPassword
        );

        if (success)
        {
            // 更新本地记录
            originalUsername = newUsername;
            originalGender = gender;
            originalAge = newAge;

            // 刷新展示
            LoadUserInfo();
            CloseEditPanel();

            // 成功提示（可以用一个临时提示，这里简单用Debug）
            Debug.Log("用户信息修改成功");
        }
        else
        {
            ShowEditTips("用户名已存在或修改失败", false);
        }
    }

    private void ShowEditTips(string msg, bool isSuccess)
    {
        textEditTips.text = msg;
        textEditTips.color = isSuccess ? new Color(0, 1, 0) : new Color(1, 0.3f, 0);
        textEditTips.gameObject.SetActive(true);
    }

    // 退出游戏
    private void OnExitGameClick()
    {
        panelExitConfirm.SetActive(true);
    }

    private void OnConfirmExit()
    {
        AudioManager.Instance.StopAllAudio();
        UserDataManager.Instance.ClearCurrentUser();
        SceneManager.LoadScene("LoginScene");
    }
    private void OnCancelExit()
    {
        panelExitConfirm.SetActive(false);
    }

    private void OnDestroy()
    {
        btnSettings.onClick.RemoveAllListeners();
        btnCloseSettings.onClick.RemoveAllListeners();
        btnExitGame.onClick.RemoveAllListeners();
        btnConfirmExit.onClick.RemoveAllListeners();
        btnCancelExit.onClick.RemoveAllListeners();
        btnEditInfo.onClick.RemoveAllListeners();
        btnSaveEdit.onClick.RemoveAllListeners();
        btnCancelEdit.onClick.RemoveAllListeners();
        sliderBGM.onValueChanged.RemoveAllListeners();
        sliderWave.onValueChanged.RemoveAllListeners();
    }
}