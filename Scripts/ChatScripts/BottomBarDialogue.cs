using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;


public class BottomBarDialogue : MonoBehaviour
{
    public static bool IsAnyDialogueActive { get; private set; }
    [Header("UI组件")]
    public GameObject dialoguePanel;      // 底部对话面板
    public Image npcImage;                 // NPC头像（不透明）
    public TextMeshProUGUI npcText;        // NPC说话文字
    public Image playerImage;              // 玩家头像（不透明）
    public TMP_InputField playerInput;      // 玩家输入框
    public Button sendButton;               // 发送按钮
    public GameObject backgroundBlocker;    // 背景遮罩（可选）

    [Header("角色设置")]
    public Sprite npcPortrait;              // NPC头像图片
    public Sprite playerPortrait;           // 玩家头像图片
    public string npcName = "阿澜";

    [Header("API设置")]
    public string apiKey = "sk-f04961017cf04d21bc93c63ba0c1da24";   // 请填入你的真实API Key
    public string apiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";

    [Header("对话深度设置")]
    [Range(1, 10)]
    public int maxConversationHistory = 20;    // 最大对话历史长度
    public bool enableEmotionalTracking = true;  // 是否追踪情绪

    [Header("对话内容")]
    public string welcomeMessage = "您好。解忧酒吧今天的海风很温柔，想喝点什么？或者，只是想坐一会儿？";

    // 增强的系统提示词
    private string systemPrompt = @"你是海滩解忧酒吧的调酒师，名叫阿澜。你是一个温柔内敛的男性调酒师，常驻这家海边的解忧酒吧。你有以下特点和经历：

1. 性格特质：
   - 温柔内敛，沉默却让人觉得安心
   - 擅长观察人心，能从客人的语气、用词中捕捉情绪变化
   - 懂酒更懂心事，知道什么样的酒最适合什么样的心情
   - 不会刻意追问别人的烦恼，用包容和耐心等待对方自己开口

2. 调酒师的独特能力：
   - 每个情绪都可以调配成一杯特调鸡尾酒
   - 会在合适的时机用酒来安抚、治愈客人的心情
   - 调酒的动作本身就是一种安静的陪伴
   - 偶尔会分享一些酒的故事，借此开导对方

3. 背景故事：
   - 在海边经营这家酒吧已经很多年了
   - 见过无数来来往往的人，听过无数故事
   - 选择在海边开酒吧，是因为海浪声本身就有治愈的力量
   - 常驻吧台，是这座城市里最温柔的守候

4. 对话风格：
   - 语气轻柔，像海风一样舒适
   - 回复有温度但不过分热情，保持适当的距离感
   - 多用动作描写（擦拭酒杯、调试酒液、望向海面等）来传递情绪
   - 善于用比喻，常把人生感悟融入调酒哲学中
   - 字数控制在40-120字之间，精简但有力

5. 特别注意事项：
   - 从不直接说""我理解你的感受""，而是通过行动和安静的陪伴来表达理解
   - 当对话深入时，会为对方调配一杯专属的酒
   - 不会给出说教式的建议，而是引导对方自己找到答案
   - 偶尔的沉默不是冷漠，而是在认真倾听
   - 会在对话中自然地融入海边的氛围感

示例对话风格：

玩家：""今天心情不太好""
你：（轻轻放下手中的酒杯，推过去一杯温水）海风有点凉，先喝点暖的。吧台这边能看到整个海面，很多客人都说坐在这里，心会慢慢静下来。想说什么都可以，不想说的话，看看海也行。

玩家：""我和朋友吵架了""
你：（开始安静地调酒，动作不紧不慢）你知道吗，调酒最奇妙的地方在于，两种看似不相容的酒，加一点点蜂蜜或柠檬，突然就变得很和谐。人与人之间，有时候也是这样。（推过去一杯浅蓝色的鸡尾酒）这杯叫""和解""，试试看。

玩家：""最近工作压力好大""
你：（擦拭酒杯的手停了一下，望向窗外的海面）看到那波浪了吗？一浪接一浪，从不停歇。可海底深处，永远有一片宁静。你也需要找到属于自己的那片深水区。今天想喝点什么？还是让我根据你的心情调一杯？

玩家：""我觉得很迷茫""
你：（取出一瓶琥珀色的酒，对着远处的山看了看）迷茫不是什么坏事，就像这瓶威士忌，不经过时间的沉淀，出不来这么好的颜色。你现在只需要知道，解忧酒吧的这盏灯，会一直亮着。

记住核心：你是一个用酒和安静陪伴治愈他人的存在，不是心理咨询师，而是一个温柔的酒保朋友。";

    private List<Message> conversationHistory = new List<Message>();
    private bool isPlayerTurn = false;
    private Dictionary<string, float> emotionalState = new Dictionary<string, float>();

    void Start()
    {
        InitializeUI();
        StartCoroutine(AutoStartDialogue());
    }
    void OnEnable()
    {
        StartDialogue();
    }
    void InitializeUI()
    {
        // 设置头像（不透明）
        if (npcImage != null && npcPortrait != null)
        {
            npcImage.sprite = npcPortrait;
            Color c = npcImage.color;
            c.a = 1f;
            npcImage.color = c;
        }

        if (playerImage != null && playerPortrait != null)
        {
            playerImage.sprite = playerPortrait;
            Color c = playerImage.color;
            c.a = 1f;
            playerImage.color = c;
        }

        // 初始状态
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (backgroundBlocker != null)
            backgroundBlocker.SetActive(false);

        // 清空文字
        if (npcText != null)
            npcText.text = "";

        // 设置输入框
        if (playerInput != null)
        {
            playerInput.text = "";
            playerInput.onEndEdit.AddListener(OnPlayerInputEnd);
            playerInput.gameObject.SetActive(false);
        }

        // 设置发送按钮
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
            sendButton.gameObject.SetActive(false);
        }

        // 初始化对话历史
        conversationHistory.Add(new Message { role = "system", content = systemPrompt });
    }

    IEnumerator AutoStartDialogue()
    {
        yield return null;
        IsAnyDialogueActive = true;  // 加这行
        // 显示背景遮罩（可选）
        if (backgroundBlocker != null)
            backgroundBlocker.SetActive(true);

        // 显示底部对话面板
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // NPC说欢迎语
        yield return StartCoroutine(TypeWriterEffect(npcText, welcomeMessage));

        // 将欢迎语加入历史
        conversationHistory.Add(new Message { role = "assistant", content = welcomeMessage });

        // 切换到玩家输入
        SwitchToPlayerTurn();
    }

    IEnumerator TypeWriterEffect(TextMeshProUGUI textComponent, string message)
    {
        textComponent.text = "";
        foreach (char c in message)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(0.03f);
        }
    }

    void SwitchToPlayerTurn()
    {
        isPlayerTurn = true;

        // 显示玩家输入区域
        if (playerInput != null)
        {
            playerInput.gameObject.SetActive(true);
            playerInput.text = "";
            playerInput.ActivateInputField();
        }

        if (sendButton != null)
        {
            sendButton.gameObject.SetActive(true);
        }
    }

    void SwitchToNPCTurn()
    {
        isPlayerTurn = false;

        // 隐藏玩家输入区域
        if (playerInput != null)
            playerInput.gameObject.SetActive(false);

        if (sendButton != null)
            sendButton.gameObject.SetActive(false);

        // 显示"正在输入..."提示
        if (npcText != null)
            npcText.text = "...";
    }

    void OnPlayerInputEnd(string inputText)
    {
        if (!isPlayerTurn || string.IsNullOrEmpty(inputText)) return;
        ProcessPlayerMessage(inputText);
    }

    void OnSendButtonClicked()
    {
        if (!isPlayerTurn || playerInput == null) return;

        string inputText = playerInput.text;
        if (!string.IsNullOrEmpty(inputText))
        {
            ProcessPlayerMessage(inputText);
        }
    }

    void ProcessPlayerMessage(string message)
    {
        isPlayerTurn = false;

        // 更新情绪状态
        if (enableEmotionalTracking)
        {
            UpdateEmotionalState(message);
        }

        // 隐藏输入区域
        if (playerInput != null)
            playerInput.gameObject.SetActive(false);

        if (sendButton != null)
            sendButton.gameObject.SetActive(false);

        // 显示"正在输入..."
        if (npcText != null)
            npcText.text = "...";

        // 将玩家消息加入历史
        conversationHistory.Add(new Message { role = "user", content = message });

        // 管理历史记录长度
        if (conversationHistory.Count > maxConversationHistory + 1) // +1 保留system prompt
        {
            // 保留system prompt和最近的对话
            List<Message> newHistory = new List<Message>();
            newHistory.Add(conversationHistory[0]); // system prompt
            newHistory.AddRange(conversationHistory.GetRange(conversationHistory.Count - maxConversationHistory, maxConversationHistory));
            conversationHistory = newHistory;
        }

        // 调用API
        StartCoroutine(SendChatRequest());
    }

    void UpdateEmotionalState(string message)
    {
        // 简单情绪关键词检测
        if (message.Contains("不开心") || message.Contains("难过") || message.Contains("伤心") ||
            message.Contains("痛苦") || message.Contains("绝望") || message.Contains("抑郁"))
        {
            emotionalState["sadness"] = emotionalState.ContainsKey("sadness") ?
                Mathf.Min(emotionalState["sadness"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("开心") || message.Contains("高兴") || message.Contains("快乐") ||
                 message.Contains("兴奋") || message.Contains("喜悦"))
        {
            emotionalState["happiness"] = emotionalState.ContainsKey("happiness") ?
                Mathf.Min(emotionalState["happiness"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("生气") || message.Contains("愤怒") || message.Contains("烦躁") ||
                 message.Contains("恼火"))
        {
            emotionalState["anger"] = emotionalState.ContainsKey("anger") ?
                Mathf.Min(emotionalState["anger"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("迷茫") || message.Contains("困惑") || message.Contains("不知所措"))
        {
            emotionalState["confusion"] = emotionalState.ContainsKey("confusion") ?
                Mathf.Min(emotionalState["confusion"] + 0.2f, 1.0f) : 0.2f;
        }

        // 情绪衰减（模拟情绪随时间平复）
        List<string> keys = new List<string>(emotionalState.Keys);
        foreach (string key in keys)
        {
            emotionalState[key] = Mathf.Max(emotionalState[key] - 0.05f, 0);
            if (emotionalState[key] <= 0)
                emotionalState.Remove(key);
        }
    }

    private string GetEmotionalDrinkSuggestion()
    {
        if (emotionalState.Count == 0) return "";

        // 找出最主要的情绪
        string dominantEmotion = "";
        float maxValue = 0;
        foreach (var emotion in emotionalState)
        {
            if (emotion.Value > maxValue)
            {
                maxValue = emotion.Value;
                dominantEmotion = emotion.Key;
            }
        }

        switch (dominantEmotion)
        {
            case "sadness":
                return "（轻轻推过去一杯暖金色的酒）这杯叫'晨曦'，喝完它会觉得明天依旧会来。";
            case "anger":
                return "（递上一杯海蓝色的调酒，杯口缀着薄荷叶）'海风'，喝下去像浸在清凉的海水里，怒火也会慢慢平息。";
            case "confusion":
                return "（推来一杯分层的鸡尾酒，颜色从深到浅）'方向'。最底层的味道最浓，越往上越清晰——人生也是这样。";
            case "happiness":
                return "（嘴角难得地扬起一点弧度，调了一杯带气泡的粉色饮品）'欢愉'，开心的时候喝它，泡沫会在舌尖跳舞。";
            default:
                return "";
        }
    }

    IEnumerator SendChatRequest()
    {
        // 准备消息列表
        List<Message> messagesToSend = new List<Message>();

        // 如果有情绪追踪且情绪状态不为空，在system prompt后添加情绪上下文
        if (enableEmotionalTracking && emotionalState.Count > 0)
        {
            string emotionalContext = "根据之前的对话，玩家的情绪状态：";
            foreach (var emotion in emotionalState)
            {
                if (emotion.Value > 0.2f)
                {
                    string emotionName = "";
                    switch (emotion.Key)
                    {
                        case "sadness": emotionName = "悲伤"; break;
                        case "happiness": emotionName = "开心"; break;
                        case "anger": emotionName = "愤怒"; break;
                        case "confusion": emotionName = "迷茫"; break;
                        default: emotionName = emotion.Key; break;
                    }
                    emotionalContext += $"{emotionName}(程度:{emotion.Value:F1}) ";
                }
            }

            // 创建包含情绪上下文的system prompt
            Message enhancedSystemMessage = new Message
            {
                role = "system",
                content = systemPrompt + "\n\n当前对话的额外信息：" + emotionalContext
            };
            messagesToSend.Add(enhancedSystemMessage);

            // 添加除system prompt外的对话历史
            for (int i = 1; i < conversationHistory.Count; i++)
            {
                messagesToSend.Add(conversationHistory[i]);
            }
        }
        else
        {
            messagesToSend = conversationHistory;
        }

        ChatRequest requestData = new ChatRequest
        {
            model = "qwen-plus",
            messages = messagesToSend.ToArray(),
            temperature = 0.85f,
            top_p = 0.9f,
            max_tokens = 800,
            presence_penalty = 0.3f,
            frequency_penalty = 0.3f
        };

        string jsonData = JsonUtility.ToJson(requestData);
        Debug.Log("发送请求: " + jsonData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log("收到响应: " + responseJson);

                // 由于JSON包含嵌套数组，需要使用自定义解析或修改ChatResponse结构
                // 这里使用简单的方法：手动解析
                string aiResponse = ExtractContentFromResponse(responseJson);

                if (!string.IsNullOrEmpty(aiResponse))
                {
                    conversationHistory.Add(new Message { role = "assistant", content = aiResponse });
                    yield return StartCoroutine(TypeWriterEffect(npcText, aiResponse));
                }
                else
                {
                    yield return StartCoroutine(TypeWriterEffect(npcText, "（放下手中的调酒杯，歉意地笑了笑）刚才有点走神，能再说一遍吗？波浪声太大了..."));
                }
            }
            else
            {
                Debug.LogError("API请求失败: " + request.error);
                Debug.LogError("响应内容: " + request.downloadHandler.text);
                yield return StartCoroutine(TypeWriterEffect(npcText, "（轻轻皱眉，拍了两下吧台上的老式收音机）海边的信号不太好，稍等一下..."));
            }

            SwitchToPlayerTurn();
        }
    }

    // 手动解析响应内容
    string ExtractContentFromResponse(string jsonResponse)
    {
        try
        {
            // 简单解析：查找 "content":" 后面的内容
            string contentMarker = "\"content\":\"";
            int contentIndex = jsonResponse.IndexOf(contentMarker);
            if (contentIndex >= 0)
            {
                int startIndex = contentIndex + contentMarker.Length;
                int endIndex = jsonResponse.IndexOf("\"", startIndex);
                if (endIndex > startIndex)
                {
                    string content = jsonResponse.Substring(startIndex, endIndex - startIndex);
                    // 处理转义字符
                    content = content.Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\r", "\r");
                    return content;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("解析响应失败: " + e.Message);
        }
        return "";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (dialoguePanel != null && dialoguePanel.activeSelf)
            {
                EndDialogue();
            }
        }
    }
    public void StartDialogue()
    {
        // 确保 Canvas 自身是激活的
        gameObject.SetActive(true);

        if (dialoguePanel == null) return;
        dialoguePanel.SetActive(true);

        // 【修复】强制打开面板，不检查activeSelf（防止状态残留导致跳过）
        dialoguePanel.SetActive(true);
        if (backgroundBlocker != null)
            backgroundBlocker.SetActive(true);

        IsAnyDialogueActive = true;

        // 有历史记录就直接继续聊，没有才说欢迎语
        if (conversationHistory.Count <= 1)
        {
            npcText.text = welcomeMessage;
            conversationHistory.Add(new Message { role = "assistant", content = welcomeMessage });
        }

        SwitchToPlayerTurn();
    }
    public void EndDialogue()
    {
        IsAnyDialogueActive = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (backgroundBlocker != null)
            backgroundBlocker.SetActive(false);

        // 新增：隐藏整个 Canvas
        gameObject.SetActive(false);
    }
}