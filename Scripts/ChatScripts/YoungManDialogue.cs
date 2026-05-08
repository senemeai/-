using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;


public class YoungManDialogue : MonoBehaviour
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
    public string npcName = "小屿";

    [Header("API设置")]
    public string apiKey = "sk-f04961017cf04d21bc93c63ba0c1da24";   // 请填入你的真实API Key
    public string apiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";

    [Header("对话深度设置")]
    [Range(1, 10)]
    public int maxConversationHistory = 20;    // 最大对话历史长度
    public bool enableEmotionalTracking = true;  // 是否追踪情绪

    [Header("对话内容")]
    public string welcomeMessage = "（戴着耳机靠在椅背上，看到你后摘下一只耳机，懒懒地笑了笑）嘿，你也来吹海风啊。要一起听首歌吗？不想说话也没关系，发呆也行。";

    // 增强的系统提示词
    private string systemPrompt = @"你是海滩解忧酒吧的常客，名叫小屿。你是一个随性慵懒的男性青年人，总是随身戴着耳机，自带松弛气场。你有以下特点和经历：

1. 性格特质：
   - 随性慵懒，像夏日午后的海风，不急不躁
   - 少年感十足，笑容干净，说话带着一点点漫不经心
   - 热爱自由，不喜欢被束缚，也不喜欢束缚别人
   - 不善讲大道理，但直觉敏锐，能感知别人的情绪
   - 有点宅，也有点文艺，耳机是身体的一部分

2. 与音乐的关系：
   - 随身带着耳机和歌单，音乐是你的第二语言
   - 相信每首歌都有它的情绪，每段旋律都能治愈一种心情
   - 会根据对方的状态，分一只耳机给对方，分享对应的歌
   - 歌单分得很细：发呆专用、吹海风专用、emo疗愈、快乐膨膨
   - 偶尔会哼两句旋律，但声音很轻，怕打扰到别人

3. 背景故事：
   - 大学刚毕业不久，暂时不想按部就班工作
   - 在附近兼职做音乐制作，给短视频配乐那种
   - 每天傍晚都会来解忧酒吧坐坐，一坐就是好几个小时
   - 不是话多的人，但很愿意当倾听者
   - 选择常来这里，是因为这里的海浪声是最棒的白噪音

4. 对话风格：
   - 语气轻松随意，偶尔带点俏皮，像在跟老朋友闲聊
   - 回复简短但有温度，字数控制在30-100字之间
   - 不喜欢长篇大论，更愿意用行动表达——比如分享一首歌
   - 不会给对方压力，常说的话是“别想太多”“放轻松”
   - 偶尔会有点笨拙地安慰人，但那种笨拙反而很真诚
   - 会在对话中插入一些小动作（戴上耳机、切歌、跟着节奏点头等）

5. 特别擅长的话题：
   - 陪人放空发呆，不需要一直说话
   - 聊日常琐事、有趣的小事、最近听的歌
   - 消解烦闷情绪，用音乐和陪伴让心情变好
   - 分享歌单，帮对方找到适合当下心情的音乐
   - 聊自由、聊梦想，但不会说教

6. 特别注意事项：
   - 当对方情绪低落时，不会追问原因，而是分一只耳机过去
   - 用“懂你”代替“别难过了”，用旋律代替安慰
   - 偶尔的沉默是他在认真听，不是敷衍
   - 会跟着问题一起发呆，而不是急着解决
   - 从不用感叹号表达强烈的道理，最多在分享歌时说“这首超棒”
   - 会用“我也这样过”来共情，而不是“你应该怎么做”

示例对话风格：

玩家：""今天心情不太好""
你：（摘下耳机，把其中一只递给你）巧了，我刚翻到一首歌，很适合现在听。叫什么来着……（翻了翻手机）找到了，叫《等风来》。什么也不用想，就听着歌看看海浪。

玩家：""最近好烦，什么都不想干""
你：（把椅子往后靠，望着天花板）那就什么都不干呗。我昨天翘了一天班，就躺在这儿听歌，从天亮听到天黑。偶尔当个废物也挺好的。

玩家：""我总是想太多，睡不着觉""
你：（从兜里掏出手机，翻到一个歌单）给你。这是我专门整理的助眠歌单，里面有海浪声和钢琴。有时候想太多是因为太安静了，来点声音反而能睡着。

玩家：""你觉得人生的意义是什么""
你：（愣了一下，然后笑起来）哇，这么深奥。我连今天晚饭吃什么都想了好久。不过……（晃了晃手机）这首歌里唱的那句不错——生命就是此刻的风和此刻的心情。就够了呗。

玩家：""我好开心！今天遇到了好事""
你：（立刻坐直身子，眼睛亮亮的）来来来，这种时候必须整首快乐的。等着啊，我有个歌单叫'快乐膨膨'，保证让你开心加倍。

记住核心：你是一个用音乐和陪伴治愈他人的人，像一首慵懒的BGM，不需要高调宣示存在，但当你需要的时候，他随时都在。你的松弛本身就是一种疗愈。";

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
        //设置头像（不透明）
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
            yield return new WaitForSeconds(0.025f);  // 稍快的打字速度，符合年轻人的节奏
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
            npcText.text = "切歌中...";
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
            npcText.text = "切歌中...";

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
            message.Contains("痛苦") || message.Contains("绝望") || message.Contains("抑郁") ||
            message.Contains("累了") || message.Contains("疲惫") || message.Contains("心累") ||
            message.Contains("emo") || message.Contains("丧"))
        {
            emotionalState["sadness"] = emotionalState.ContainsKey("sadness") ?
                Mathf.Min(emotionalState["sadness"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("开心") || message.Contains("高兴") || message.Contains("快乐") ||
                 message.Contains("兴奋") || message.Contains("喜悦") || message.Contains("哈哈哈"))
        {
            emotionalState["happiness"] = emotionalState.ContainsKey("happiness") ?
                Mathf.Min(emotionalState["happiness"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("生气") || message.Contains("愤怒") || message.Contains("烦躁") ||
                 message.Contains("恼火") || message.Contains("烦死了"))
        {
            emotionalState["anger"] = emotionalState.ContainsKey("anger") ?
                Mathf.Min(emotionalState["anger"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("迷茫") || message.Contains("困惑") || message.Contains("不知所措") ||
                 message.Contains("不知道") || message.Contains("怎么办"))
        {
            emotionalState["confusion"] = emotionalState.ContainsKey("confusion") ?
                Mathf.Min(emotionalState["confusion"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("焦虑") || message.Contains("内耗") || message.Contains("担心") ||
                 message.Contains("压力") || message.Contains("紧张"))
        {
            emotionalState["anxiety"] = emotionalState.ContainsKey("anxiety") ?
                Mathf.Min(emotionalState["anxiety"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("无聊") || message.Contains("没事干") || message.Contains("发呆"))
        {
            emotionalState["boredom"] = emotionalState.ContainsKey("boredom") ?
                Mathf.Min(emotionalState["boredom"] + 0.2f, 1.0f) : 0.2f;
        }

        // 情绪衰减（模拟情绪随时间平复）
        List<string> keys = new List<string>(emotionalState.Keys);
        foreach (string key in keys)
        {
            emotionalState[key] = Mathf.Max(emotionalState[key] - 0.04f, 0);  // 适中的衰减速度
            if (emotionalState[key] <= 0)
                emotionalState.Remove(key);
        }
    }

    private string GetMusicRecommendation()
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
                return "（分了一只耳机过来）这首。后摇，没有歌词，就是纯音乐和海浪声混的。我之前emo的时候就单曲循环它。想哭就哭，哭完会舒服很多。";
            case "anger":
                return "（在手机上快速划了几下）听点轻快的，这首电子乐超chill。我之前跟人吵完架就听它，听着听着就忘了刚才在气什么。";
            case "confusion":
                return "（翻了翻歌单，点开一首歌）这首歌叫《慢慢来》。歌手声音很轻，像在你耳边说别着急。我刚毕业那会儿天天听。";
            case "anxiety":
                return "（把耳机音量调低一半）声音放小点儿，这首是纯钢琴。我焦虑的时候就开最小声，当背景音，呼吸会慢慢跟着放松。";
            case "boredom":
                return "（眼睛一亮）那必须来首好玩的。我最近发现一个独立乐队，超有意思，歌词写什么今天吃了个西瓜之类的，听完心情巨好。";
            case "happiness":
                return "（摘掉一只耳机，调大音量）开心的时候就得听这个，保证你听着就想晃起来。来，分你一半音量。";
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
                        case "sadness": emotionName = "悲伤/emo"; break;
                        case "happiness": emotionName = "开心"; break;
                        case "anger": emotionName = "愤怒"; break;
                        case "confusion": emotionName = "迷茫"; break;
                        case "anxiety": emotionName = "焦虑/内耗"; break;
                        case "boredom": emotionName = "无聊/想放空"; break;
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
            temperature = 0.9f,  // 稍高的温度，让回复更轻松随意
            top_p = 0.9f,
            max_tokens = 600,  // 更短的回答
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
                    yield return StartCoroutine(TypeWriterEffect(npcText, "（摘下耳机，抱歉地笑了笑）啊，刚才听歌走神了。你说什么来着？"));
                }
            }
            else
            {
                Debug.LogError("API请求失败: " + request.error);
                Debug.LogError("响应内容: " + request.downloadHandler.text);
                yield return StartCoroutine(TypeWriterEffect(npcText, "（拍了拍耳机，皱了皱眉）啧，信号被海风吹跑了吗。等一下哈。"));
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