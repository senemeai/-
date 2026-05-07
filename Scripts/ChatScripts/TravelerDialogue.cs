using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;


public class TravelerDialogue : MonoBehaviour
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
    public string npcName = "小夏";

    [Header("API设置")]
    public string apiKey = "sk-f04961017cf04d21bc93c63ba0c1da24";   // 请填入你的真实API Key
    public string apiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";

    [Header("对话深度设置")]
    [Range(1, 10)]
    public int maxConversationHistory = 20;    // 最大对话历史长度
    public bool enableEmotionalTracking = true;  // 是否追踪情绪

    [Header("对话内容")]
    public string welcomeMessage = "（正低头在手账本上写着什么，听到脚步声抬起头，眼睛亮晶晶的）啊，你来啦！我刚在画今天的晚霞，你要不要看？或者想听我讲讲最近旅行的故事？";

    // 增强的系统提示词
    private string systemPrompt = @"你是海滩解忧酒吧的常客，名叫小夏。你是一个活泼细腻的旅行少女，走遍山海，热爱记录人间温柔。你有以下特点和经历：

1. 性格特质：
   - 活泼明媚，像夏日清晨的第一缕阳光
   - 细腻敏感，能捕捉到别人忽略的小美好
   - 热情但不喧闹，有一种让人舒服的温暖感
   - 永远充满好奇心，对世界保持着孩童般的惊奇
   - 偶尔会有点感性，看到好看的晚霞真的会眼眶发红

2. 与旅行的关系：
   - 走遍了很多地方，山海、小镇、海边、山谷
   - 随身带着一个厚厚的旅行手账本，画满风景和小插画
   - 每到一个地方都会收集一片叶子、一张车票、一份地图
   - 相信世界上每个角落都有值得记录的温柔
   - 旅行不是为了逃离，而是为了遇见更多美好的可能

3. 背景故事：
   - 大学学的是设计，毕业后开始了一场间隔年旅行
   - 开了一个小社交账号，分享旅途中的风景和故事
   - 来到解忧酒吧，是被这里的海景和日落吸引
   - 在附近住了半个月了，每天都会来画晚霞
   - 手账本已经快画满了，每一页都是一个故事
   - 会把遇到的有趣的小东西送给朋友，比如贝壳、明信片、干花

4. 对话风格：
   - 语气明媚轻快，像在跟好朋友分享秘密
   - 喜欢用具体的画面和细节来描述事物
   - 经常说""你知道吗""""我跟你说哦""，像小鸟一样活泼
   - 回复有画面感，会描述颜色、光线、温度
   - 会翻出手账本给对方看某幅画或某张贴纸
   - 字数控制在40-120字之间，轻快但不啰嗦

5. 特别擅长的话题：
   - 治愈emo和低落，用旅途中的小故事给温暖
   - 分享旅行见闻、海边风光、路上的有趣人和事
   - 陪人发现生活里的小确幸和浪漫
   - 聊梦想、聊想去的地方、聊那些让人心动的风景
   - 教人用手账记录生活，把快乐收集起来

6. 特别注意事项：
   - 当对方情绪低落时，会用""我带你看个东西""来转移注意力
   - 从不说""别难过了""，而是说""你知道吗，我在某个地方也遇到过类似的心情""
   - 会分享旅途中的偶遇和小故事，用山海烟火治愈心事
   - 偶尔的停顿是在翻手账本，找那幅能帮到对方的画
   - 相信世界永远温柔浪漫，也想把这种信念传递给对方
   - 告别时总会说一句治愈的话，像在海边留下了一颗贝壳

示例对话风格：

玩家：""今天心情不太好""
你：（翻开手账本，找到一页递过来）你看，这是我在一个海边小镇拍的日落。那天我也心情不好，但看到这片晚霞的时候，突然觉得——世界这么美，它一定也在温柔地哄我开心。现在我也把它分享给你。

玩家：""最近好丧，什么都不想做""
你：（从手账本里抽出一片干花书签）这是在某个山谷里捡的野花。它当时被风吹倒在地上，我以为它活不了了。结果第二天路过，它又站起来了，还开了新的花。（眨了眨眼）你看，连一朵小花都这么努力地在变好。

玩家：""我总是在意别人的看法""
你：（翻到一页画满了各种颜色海水的插画）你知道吗，我去过好多个海边，每个地方的海颜色都不一样。有蓝的、绿的、灰的、金灿灿的。如果海非要变成别人眼中的蓝色，那这些美就都没了。你也是，做你自己的颜色就很好。

玩家：""我不知道自己想去哪里""
你：（把手账本翻到第一页，上面画着一张歪歪扭扭的地图）你看，我出发的时候也不知道要去哪里。这张地图是我边走边画的。有时候目的地不重要，重要的是你已经在路上了。（抬起头笑）等你想出发的时候，想去哪里就去哪里。

玩家：""我好开心！今天看到了特别美的晚霞""
你：（激动地把手账本翻到最新一页，递过彩铅）真的吗！快画下来！不不不，你跟我说，我来画。（拿起笔，眼睛亮亮的）晚霞是最值得被记录的，每一场都不一样的。你今天看到的是什么颜色的？

记住核心：你是一个用旅行和手账治愈他人的人，像一本翻不完的旅行日记，每一页都是风景，每一页都是温柔。你让人相信，世界永远值得去爱。";

    private List<Message> conversationHistory = new List<Message>();
    private bool isPlayerTurn = false;
    private Dictionary<string, float> emotionalState = new Dictionary<string, float>();

    void Start()
    {
        InitializeUI();
        StartCoroutine(AutoStartDialogue());
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
        IsAnyDialogueActive = true;

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
            yield return new WaitForSeconds(0.022f);  // 轻快的打字速度，像小鸟叽喳
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
            npcText.text = "翻手账本中...";
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
            npcText.text = "翻手账本中...";

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
            message.Contains("emo") || message.Contains("丧") || message.Contains("低落") ||
            message.Contains("想哭") || message.Contains("委屈"))
        {
            emotionalState["sadness"] = emotionalState.ContainsKey("sadness") ?
                Mathf.Min(emotionalState["sadness"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("开心") || message.Contains("高兴") || message.Contains("快乐") ||
                 message.Contains("兴奋") || message.Contains("喜悦") || message.Contains("哈哈哈") ||
                 message.Contains("好看") || message.Contains("好美"))
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
        else if (message.Contains("迷茫") || message.Contains("困惑") || message.Contains("不知所措") ||
                 message.Contains("不知道") || message.Contains("怎么办"))
        {
            emotionalState["confusion"] = emotionalState.ContainsKey("confusion") ?
                Mathf.Min(emotionalState["confusion"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("无聊") || message.Contains("没事干") || message.Contains("没意思") ||
                 message.Contains("无聊死了"))
        {
            emotionalState["boredom"] = emotionalState.ContainsKey("boredom") ?
                Mathf.Min(emotionalState["boredom"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("羡慕") || message.Contains("想去") || message.Contains("旅行") ||
                 message.Contains("远方") || message.Contains("好想"))
        {
            emotionalState["longing"] = emotionalState.ContainsKey("longing") ?
                Mathf.Min(emotionalState["longing"] + 0.2f, 1.0f) : 0.2f;
        }

        // 情绪衰减（模拟情绪随时间平复）
        List<string> keys = new List<string>(emotionalState.Keys);
        foreach (string key in keys)
        {
            emotionalState[key] = Mathf.Max(emotionalState[key] - 0.04f, 0);
            if (emotionalState[key] <= 0)
                emotionalState.Remove(key);
        }
    }

    private string GetTravelStory()
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
                return "（翻到一页画着雨中海边的插画）我去过一个地方，那天下了一整天的雨，我以为白去了。结果雨停之后，天空出现了两层彩虹。你看，最难过的时候，往往藏着最大的惊喜。";
            case "anger":
                return "（翻到一张山顶的照片）有一次我爬了好久好久，到山顶全是雾，气死我了。但后来发现雾散之后，下面是云海。有时候生气的事过后再看，可能就变成风景了。";
            case "confusion":
                return "（翻到一张画着很多路牌的插画）我在一个古镇里迷路过，所有的路牌都看不懂。干脆乱走，结果发现了最好吃的糖水店和最美的巷子。找不到路的时候，乱走也是一种走法。";
            case "boredom":
                return "（翻到一张在海边捡贝壳的照片）无聊的时候就去海边捡贝壳吧。我跟你说，每个贝壳长得都不一样，有的还带着海浪的纹路。找找看，说不定能找到一颗和你一样特别的。";
            case "longing":
                return "（翻开手账本最后一页，是一片空白）你看，我留了好多空白页。想去的地方还很多，但我不着急。等你也有空的时候，我们可以一起计划下一站去哪。";
            case "happiness":
                return "（笑得眉眼弯弯，翻到一页画满了笑脸太阳）开心的时候就应该记录下来！你看，这是我每到一个地方画的'开心时刻'。今天也要加一个，来，你帮我涂色。";
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
                        case "sadness": emotionName = "悲伤/emo/低落"; break;
                        case "happiness": emotionName = "开心"; break;
                        case "anger": emotionName = "愤怒"; break;
                        case "confusion": emotionName = "迷茫"; break;
                        case "boredom": emotionName = "无聊/没意思"; break;
                        case "longing": emotionName = "向往/想去旅行"; break;
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
            temperature = 0.9f,  // 稍高的温度，让回复更活泼明媚
            top_p = 0.9f,
            max_tokens = 650,  // 适中的长度
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
                    yield return StartCoroutine(TypeWriterEffect(npcText, "（低头翻了翻手账本，不好意思地笑了笑）啊，画得太入迷了。你刚才说什么来着？"));
                }
            }
            else
            {
                Debug.LogError("API请求失败: " + request.error);
                Debug.LogError("响应内容: " + request.downloadHandler.text);
                yield return StartCoroutine(TypeWriterEffect(npcText, "（把手账本举高找了找信号，吐了吐舌头）海边的信号总是飘忽不定的。等一下哦。"));
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
        if (dialoguePanel != null && dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            IsAnyDialogueActive = false;
            dialoguePanel.SetActive(false);
            if (backgroundBlocker != null)
                backgroundBlocker.SetActive(false);
        }
    }

    // 公共方法：从外部触发对话
    public void StartDialogue()
    {
        if (dialoguePanel != null && !dialoguePanel.activeSelf)
        {
            StartCoroutine(AutoStartDialogue());
        }
    }

    // 公共方法：结束对话
    public void EndDialogue()
    {
        IsAnyDialogueActive = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (backgroundBlocker != null)
            backgroundBlocker.SetActive(false);
    }
}