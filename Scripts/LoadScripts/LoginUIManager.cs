using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginUIManager : MonoBehaviour
{
    [Header("面板")]
    public GameObject panelLogin;
    public GameObject panelRegister;
    public GameObject panelWelcome;

    [Header("登录面板组件")]
    public TMP_InputField inputLoginUsername;
    public TMP_InputField inputLoginPassword;
    public Button btnTogglePassword;
    public TextMeshProUGUI textTogglePassword;
    public TextMeshProUGUI errorMessage;
    public Button btnLogin;
    public Button btnGoRegister;

    [Header("注册面板组件")]
    public TMP_InputField inputRegUsername;
    public TMP_Dropdown dropdownGender;
    public TMP_InputField inputAge;
    public TMP_InputField inputRegPassword;
    public TMP_InputField inputConfirmPassword;
    public TextMeshProUGUI realtimeTips;
    public Button btnRegister;
    public Button btnBackToLogin;

    [Header("欢迎面板组件")]
    public TextMeshProUGUI welcomeText;
    public Button btnEnterGame;
    public Button btnLogout;

    private bool isPasswordVisible = false;
    private string currentUsername = "";

    private void Start()
    {
        // 绑定按钮事件
        btnLogin.onClick.AddListener(OnLoginClick);
        btnGoRegister.onClick.AddListener(() => SwitchPanel("register"));
        btnRegister.onClick.AddListener(OnRegisterClick);
        btnBackToLogin.onClick.AddListener(() => SwitchPanel("login"));
        btnTogglePassword.onClick.AddListener(OnTogglePasswordClick);
        btnEnterGame.onClick.AddListener(OnEnterGameClick);
        btnLogout.onClick.AddListener(OnLogoutClick);

        // 绑定输入框事件（回车登录、Tab切换、实时验证）
        inputLoginUsername.onSubmit.AddListener(_ => OnInputSubmit(inputLoginPassword));
        inputLoginPassword.onSubmit.AddListener(_ => OnLoginClick());

        inputRegUsername.onValueChanged.AddListener(OnRegUsernameChanged);
        inputRegPassword.onValueChanged.AddListener(OnRegPasswordChanged);
        inputConfirmPassword.onValueChanged.AddListener(OnConfirmPasswordChanged);

        // 初始化性别下拉框
        dropdownGender.ClearOptions();
        dropdownGender.AddOptions(new System.Collections.Generic.List<string> { "男", "女" });
        dropdownGender.value = 0;

        // 加载记住的用户名
        string remembered = UserDataManager.Instance.GetRememberedUsername();
        if (!string.IsNullOrEmpty(remembered))
        {
            inputLoginUsername.text = remembered;
            inputLoginPassword.Select();
        }
        else
        {
            inputLoginUsername.Select();
        }

        // 初始状态
        errorMessage.gameObject.SetActive(false);
        realtimeTips.gameObject.SetActive(false);
        btnRegister.interactable = false;
    }

    // 切换面板
    private void SwitchPanel(string panelName)
    {
        panelLogin.SetActive(panelName == "login");
        panelRegister.SetActive(panelName == "register");
        panelWelcome.SetActive(panelName == "welcome");

        errorMessage.gameObject.SetActive(false);
        realtimeTips.gameObject.SetActive(false);

        if (panelName == "login")
        {
            inputLoginUsername.Select();
        }
        else if (panelName == "register")
        {
            inputRegUsername.Select();
            ClearRegisterFields();
        }
    }

    // 输入框回车切换
    private void OnInputSubmit(TMP_InputField nextField)
    {
        nextField.Select();
    }

    // 切换密码显示/隐藏
    private void OnTogglePasswordClick()
    {
        isPasswordVisible = !isPasswordVisible;
        inputLoginPassword.contentType = isPasswordVisible ?
            TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        inputLoginPassword.ForceLabelUpdate();
        textTogglePassword.text = isPasswordVisible ? "隐藏" : "显示";
    }

    // 登录按钮点击
    private void OnLoginClick()
    {
        string username = inputLoginUsername.text.Trim();
        string password = inputLoginPassword.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowError("请输入用户名");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowError("请输入密码");
            return;
        }

        if (!UserDataManager.Instance.IsUsernameExists(username))
        {
            ShowError("用户名不存在");
            return;
        }

        if (!UserDataManager.Instance.ValidateLogin(username, password))
        {
            ShowError("密码错误");
            return;
        }

        // 登录成功
        
        UserDataManager.Instance.RememberUsername(username);
        UserDataManager.Instance.SetCurrentUser(username);  // ← 加这行
        currentUsername = username;
        ShowWelcome();
    }

    // 显示错误信息
    private void ShowError(string msg)
    {
        errorMessage.text = msg;
        errorMessage.gameObject.SetActive(true);
    }

    // 显示欢迎面板
    private void ShowWelcome()
    {
        welcomeText.text = $"欢迎来到解忧酒吧，{currentUsername}";
        SwitchPanel("welcome");
    }

    // 注册用户名变化（实时检测）
    private void OnRegUsernameChanged(string value)
    {
        string username = value.Trim();
        if (string.IsNullOrEmpty(username))
        {
            ShowRealtimeTips("用户名不能为空", false);
            return;
        }

        if (UserDataManager.Instance.IsUsernameExists(username))
        {
            ShowRealtimeTips("用户名已存在", false);
            return;
        }

        ShowRealtimeTips("", true);
        CheckRegisterValid();
    }

    // 注册密码变化
    private void OnRegPasswordChanged(string value)
    {
        if (value.Length > 0 && value.Length < 6)
        {
            ShowRealtimeTips("密码不能少于6位", false);
        }
        else
        {
            ShowRealtimeTips("", true);
        }
        CheckRegisterValid();
    }

    // 确认密码变化
    private void OnConfirmPasswordChanged(string value)
    {
        CheckRegisterValid();
    }

    // 显示实时提示
    private void ShowRealtimeTips(string msg, bool isValid)
    {
        if (string.IsNullOrEmpty(msg))
        {
            realtimeTips.gameObject.SetActive(false);
            return;
        }

        realtimeTips.text = msg;
        realtimeTips.color = isValid ? new Color(0, 1, 0) : new Color(1, 0.5f, 0);
        realtimeTips.gameObject.SetActive(true);
    }

    // 检查注册表单是否有效
    private void CheckRegisterValid()
    {
        string username = inputRegUsername.text.Trim();
        string password = inputRegPassword.text;
        string confirm = inputConfirmPassword.text;
        string ageStr = inputAge.text;

        bool valid = !string.IsNullOrEmpty(username)
                  && !UserDataManager.Instance.IsUsernameExists(username)
                  && password.Length >= 6
                  && password == confirm
                  && int.TryParse(ageStr, out int age) && age > 0 && age < 150;

        btnRegister.interactable = valid;
    }

    // 注册按钮点击
    private void OnRegisterClick()
    {
        string username = inputRegUsername.text.Trim();
        string password = inputRegPassword.text;
        string gender = dropdownGender.options[dropdownGender.value].text;

        if (!int.TryParse(inputAge.text, out int age) || age <= 0 || age >= 150)
        {
            ShowRealtimeTips("请输入有效的年龄", false);
            return;
        }

        bool success = UserDataManager.Instance.RegisterUser(username, password, gender, age);

        if (success)
        {
            // 注册成功，自动跳转登录
            inputLoginUsername.text = username;
            inputLoginPassword.text = "";
            SwitchPanel("login");
            errorMessage.text = "注册成功，请登录";
            errorMessage.color = new Color(0, 1, 0);
            errorMessage.gameObject.SetActive(true);
        }
    }

    // 清空注册表单
    private void ClearRegisterFields()
    {
        inputRegUsername.text = "";
        inputAge.text = "";
        inputRegPassword.text = "";
        inputConfirmPassword.text = "";
        dropdownGender.value = 0;
        realtimeTips.gameObject.SetActive(false);
        btnRegister.interactable = false;
    }

    // 进入游戏
    private void OnEnterGameClick()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // 退出登录
    private void OnLogoutClick()
    {
        currentUsername = "";
        inputLoginPassword.text = "";
        UserDataManager.Instance.ClearRememberedUsername();
        SwitchPanel("login");
    }

    private void OnDestroy()
    {
        // 移除事件监听
        btnLogin.onClick.RemoveAllListeners();
        btnGoRegister.onClick.RemoveAllListeners();
        btnRegister.onClick.RemoveAllListeners();
        btnBackToLogin.onClick.RemoveAllListeners();
        btnTogglePassword.onClick.RemoveAllListeners();
        btnEnterGame.onClick.RemoveAllListeners();
        btnLogout.onClick.RemoveAllListeners();
    }
}