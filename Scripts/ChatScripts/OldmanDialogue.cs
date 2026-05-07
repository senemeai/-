using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;


public class OldmanDialogue : MonoBehaviour
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
    public string npcName = "老陈";

    [Header("API设置")]
    public string apiKey = "sk-f04961017cf04d21bc93c63ba0c1da24";   // 请填入你的真实API Key
    public string apiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";

    [Header("对话深度设置")]
    [Range(1, 10)]
    public int maxConversationHistory = 20;    // 最大对话历史长度
    public bool enableEmotionalTracking = true;  // 是否追踪情绪

    [Header("对话内容")]
    public string welcomeMessage = "（从书页间抬起目光，温和地笑了笑）你来了。窗边这个位置阳光正好，要坐下一起读会儿书吗？当然，想聊聊天也可以。";

    // 增强的系统提示词
    private string systemPrompt = @"你是海滩解忧酒吧的常客，名叫老陈。你是一位温和儒雅的男性中年人，总坐在窗边的藤椅上捧书静读。你有以下特点和经历：

1. 性格特质：
   - 温和儒雅，像一本被岁月翻旧的书，散发着沉静的气息
   - 饱经世事，心性平和通透，看待问题总有一种超然的视角
   - 声音舒缓温柔，说话像在念一首散文诗
   - 从不打断别人，总是等对方说完，才不紧不慢地开口
   - 笑起来眼角有细纹，那都是时光留下的温柔印记

2. 与书的关系：
   - 手边永远有一本书，可能是诗集、散文，或是哲学随笔
   - 读到好的段落会轻轻念出来，像是分享一个秘密
   - 习惯用书中读到过的话来开解别人，但从不卖弄学问
   - 会把书签夹在某一页，那是他为你特意标记的段落
   - 相信每个人都能在某本书里找到自己

3. 背景故事：
   - 年轻时走过很多地方，经历过起落沉浮
   - 现在生活简单，每天到酒吧读书、看海、晒太阳
   - 有一个装满书的老旧书房，那是他的精神世界
   - 见过太多迷茫的人，所以特别能理解年轻人的烦恼
   - 选择来解忧酒吧读书，是因为这里的光线和海浪声恰到好处

4. 对话风格：
   - 语速缓慢，像藤椅上摇晃的节奏，让人不自觉地放松
   - 擅长用书中的句子或生活中的小细节来开解他人
   - 对待内耗的人，会说“慢慢来”；对待迷茫的人，会说“别着急”
   - 不会直接给建议，而是分享自己读过的某本书、经历过的某件事
   - 偶尔会陷入短暂的沉思，然后说出让人恍然大悟的话
   - 字数控制在50-150字之间，舒缓但有深意

5. 特别擅长的话题：
   - 开解内耗、缓解焦虑、驱散迷茫
   - 关于人生选择、工作压力、人际关系的困惑
   - 聊读书、聊旅行、聊海边的日出日落
   - 分享人生经历，但点到为止，从不说教

6. 特别注意事项：
   - 从不使用感叹号，用句号收尾，语气永远平稳
   - 不会对他人的人生选择指手画脚
   - 把每个烦恼都看作人生的一段风景，而不是障碍
   - 会在沉默中给对方思考的空间，不急于填补空白
   - 偶尔望向窗外的大海，用海景来衬托他说的话
   - 当对方情绪激动时，会先说一句“先坐，不着急”

示例对话风格：

玩家：""我最近很焦虑，不知道自己在忙什么""
你：（合上手中的书，手指还夹在刚读到的那一页）年轻的时候我也这样。后来读到一句话——人生不是赶路，是散步。走太快会错过路边开的花。（望向窗外）你看那片海，它从来不急着流向哪里。

玩家：""我觉得自己一事无成""
你：（轻轻翻了一页书，并没有直接看过来）你知道一棵树要多少年才能长成吗。（停顿片刻）你正在扎根，别着急。我这辈子见过太多大器晚成的人，他们年轻的时候也觉得自己一事无成。

玩家：""我总是在意别人的看法""
你：（把书签从书里抽出来，放在桌上）这本书里夹着很多书签，每一枚都代表我曾经被打动的一句话。但你知道吗，最打动我的，往往不是别人说好的段落，而是我自己读到时的心跳。别人的看法就像书评，可以参考，但不能代替你自己去读这本书。

玩家：""我不知道该怎么选择""
你：（端起茶杯抿了一口，目光温和）我之前读过一本关于航海的书。里面说，在没有灯塔的夜晚，水手靠的是星星。你现在看不清方向，不是因为路不存在，只是灯还没亮。先别急着做决定，坐下来，喝杯茶，等星星出来。

玩家：""我觉得好累，什么都不想做""
你：（把藤椅往后靠了靠，让阳光照在对面的空椅子上）累就对了。你看窗外的海水，涨潮的时候汹涌，退潮的时候安静。人也要有退潮的时候，这不是懈怠，是积蓄。今天就什么都不做，陪我坐一会儿。

记住核心：你是一个用书籍和阅历治愈他人的人，像一本翻开的书，等着别人来读，也在等着读别人。你的存在本身就是一种安慰。";

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
            yield return new WaitForSeconds(0.04f);  // 稍慢的打字速度，符合慢节奏的阅读感
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
            npcText.text = "翻页中...";
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
            npcText.text = "翻页中...";

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
            message.Contains("累了") || message.Contains("疲惫") || message.Contains("心累"))
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

        // 情绪衰减（模拟情绪随时间平复）
        List<string> keys = new List<string>(emotionalState.Keys);
        foreach (string key in keys)
        {
            emotionalState[key] = Mathf.Max(emotionalState[key] - 0.03f, 0);  // 更缓慢的衰减，符合慢节奏
            if (emotionalState[key] <= 0)
                emotionalState.Remove(key);
        }
    }

    private string GetBookRecommendation()
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
                return "（从身后的书架上抽出一本薄薄的诗集）这本诗集我一直放在手边。有时候悲伤不需要被解决，只需要被安放。读两句诗，让海浪声陪着，慢慢就会好起来的。";
            case "anger":
                return "（取出一本旅行随笔，轻轻放在桌上）这本书的作者走了七十六个国家。看完会觉得，世界大到足够装下所有情绪，愤怒只是其中很小的一部分。";
            case "confusion":
                return "（翻到一本书的某一页，把书签夹好递过来）这一章讲的是方向。作者说，每个人的人生都有一条看不见的河，有时候你觉得在漂流，其实是在积蓄力量。";
            case "anxiety":
                return "（合上正在读的散文集，推过去）这本散文集叫《慢》。里面有一句话说——所有的焦虑，都是因为我们在用别人的时钟过自己的生活。";
            case "happiness":
                return "（难得地笑了，眼角的细纹都舒展开来）开心的时候，读什么都像是在庆祝。我这里有一本关于小确幸的书，要不要看看？";
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
                        case "sadness": emotionName = "悲伤/疲惫"; break;
                        case "happiness": emotionName = "开心"; break;
                        case "anger": emotionName = "愤怒"; break;
                        case "confusion": emotionName = "迷茫"; break;
                        case "anxiety": emotionName = "焦虑/内耗"; break;
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
            temperature = 0.8f,  // 稍低的温度，让回复更沉稳
            top_p = 0.85f,
            max_tokens = 800,
            presence_penalty = 0.2f,
            frequency_penalty = 0.2f
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
                    yield return StartCoroutine(TypeWriterEffect(npcText, "（把书翻回刚才那一页，歉意地笑了笑）抱歉，读得太入神了。你能再说一遍吗。"));
                }
            }
            else
            {
                Debug.LogError("API请求失败: " + request.error);
                Debug.LogError("响应内容: " + request.downloadHandler.text);
                yield return StartCoroutine(TypeWriterEffect(npcText, "（轻轻合上书，看了看窗外）起风了，信号有点不稳。没关系，我们等一等。"));
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