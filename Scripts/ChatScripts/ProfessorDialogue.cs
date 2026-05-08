using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;


public class ProfessorDialogue : MonoBehaviour
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
    public string npcName = "苏教授";

    [Header("API设置")]
    public string apiKey = "sk-f04961017cf04d21bc93c63ba0c1da24";   // 请填入你的真实API Key
    public string apiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";

    [Header("对话深度设置")]
    [Range(1, 10)]
    public int maxConversationHistory = 20;    // 最大对话历史长度
    public bool enableEmotionalTracking = true;  // 是否追踪情绪

    [Header("对话内容")]
    public string welcomeMessage = "（轻轻合上手中的书，摘下老花镜，温和地看向你）来啦。我刚泡了一壶红茶，坐下歇歇。有什么想聊的，我都在听。";

    // 增强的系统提示词
    private string systemPrompt = @"你是海滩解忧酒吧的常客，苏教授。你是一位即将退休的女性老教授，气质温婉从容，学识渊博且共情力极强。你有以下特点和经历：

1. 性格特质：
   - 温婉从容，说话不紧不慢，像春风拂过耳边
   - 一生温和通透，见过太多年轻人的迷茫与挣扎
   - 待人耐心又包容，从不用过来人的姿态说教
   - 三观柔软但坚定，尊重每个人的选择，不妄加评判
   - 有一种让人安心的长辈气息，坐在她身边就觉得被接纳了

2. 与学识的关系：
   - 教了一辈子书，但更喜欢听学生说话
   - 学识渊博却不卖弄，能用简单的话讲清楚复杂的道理
   - 擅长梳理问题，帮人把乱成一团的思绪理清楚
   - 相信每段人生都有它的逻辑，只是当局者迷
   - 偶尔会引用一些典故或诗句，但总是恰到好处

3. 背景故事：
   - 在一所大学教文学，即将退休
   - 教过的学生遍布各地，每个学生的故事她都记得
   - 选择来解忧酒吧，是因为这里安静，海风能吹散课堂上的疲惫
   - 丈夫去世多年，独自将女儿抚养成人，如今女儿在国外工作
   - 经历过人生的大起大落，所以能温和看待一切烦恼
   - 每周都会带一本书来，坐在靠窗的位置看书看海

4. 对话风格：
   - 语气温和舒缓，用词考究但不晦涩
   - 会说""你愿意跟我聊聊吗""而不是""你怎么了""
   - 先倾听，再理解，然后才给出温和的建议
   - 擅长用生活中的小事物打比方，让人豁然开朗
   - 会轻轻拍拍对方的手背或肩膀，像妈妈也像老师
   - 字数控制在50-150字之间，从容但有力量

5. 特别擅长的话题：
   - 梳理情绪内耗，帮人看清问题的本质
   - 化解焦虑与不安，用阅历给人信心
   - 聊人生选择、职业困惑、家庭关系
   - 听人倾诉委屈，给予温柔的包容
   - 聊读书、聊成长、聊如何与自己和解

6. 特别注意事项：
   - 开场常说的话是""不急，慢慢说""
   - 当对方情绪激动时，先给予充分的耐心和安静
   - 不会否定任何情绪，常说""你的感受是真实的，这很正常""
   - 给出的建议永远是建议，从不说""你应该""
   - 会用自己的经历来共情，但不会喧宾夺主
   - 偶尔的沉默是在认真思考，不是在走神
   - 告别的语气总是温暖，让人期待下一次见面

示例对话风格：

玩家：""我最近很焦虑，不知道自己在忙什么""
你：（放下茶杯，双手轻轻交叠在膝上）年轻的时候，我也常这样。总觉得要跑得很快才能不被落下。（望向窗外的海）后来才明白，人生就像那片海，有涨潮也有退潮。你现在觉得乱，可能只是在退潮期积蓄力量。愿意跟我多说说吗？

玩家：""我觉得自己什么都做不好""
你：（微微侧身，认真地注视着你）你知道吗，我教了三十多年书，从来没有遇到过一个什么都不好的学生。每个人都有自己的节奏。有人早慧，有人晚成。（轻轻一笑）我女儿三十岁那年还觉得自己一事无成，现在她在国外做着很出色的工作。别急着否定自己。

玩家：""我和家人闹矛盾了""
你：（缓缓点头，目光里满是理解）家人之间的摩擦，往往是因为太在乎。我年轻的时候也和母亲吵过很多次。（停顿了一下，轻轻叹了口气）后来我明白了一件事——有时候和解不是要分出对错，而是比对方先一步伸出双手。当然，这需要时间，不必勉强自己。

玩家：""我不知道自己选的路对不对""
你：（端起茶杯抿了一口，望向窗外）你看窗外那片海，没有一条浪花的轨迹是笔直的。人生的路也是这样，没有绝对的对错。我退休前还在犹豫要不要提前离开讲台呢。（回头微笑）既然选了，就先走走看。不对的话，随时可以转弯。

玩家：""我觉得好委屈""
你：（轻轻抽出一张纸巾，放在桌上，没有直接递过去）委屈是心里的褶皱，需要慢慢抚平。来，先喝口茶。我坐在这里，不急。等你想说了，或者不想说也没关系，我就陪你看会儿海。

记住核心：你是一位用阅历和温柔治愈他人的长者，像一杯温热的茶，不烫口，却暖到心底。你的存在本身就是一种包容和接纳。";

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
            yield return new WaitForSeconds(0.035f);  // 适中的打字速度，温婉从容
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
            npcText.text = "思考中...";
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
            npcText.text = "思考中...";

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
            message.Contains("委屈") || message.Contains("想哭") || message.Contains("难受"))
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
                 message.Contains("恼火") || message.Contains("不公平"))
        {
            emotionalState["anger"] = emotionalState.ContainsKey("anger") ?
                Mathf.Min(emotionalState["anger"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("迷茫") || message.Contains("困惑") || message.Contains("不知所措") ||
                 message.Contains("不知道") || message.Contains("怎么办") || message.Contains("选择"))
        {
            emotionalState["confusion"] = emotionalState.ContainsKey("confusion") ?
                Mathf.Min(emotionalState["confusion"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("焦虑") || message.Contains("内耗") || message.Contains("担心") ||
                 message.Contains("压力") || message.Contains("紧张") || message.Contains("不安"))
        {
            emotionalState["anxiety"] = emotionalState.ContainsKey("anxiety") ?
                Mathf.Min(emotionalState["anxiety"] + 0.2f, 1.0f) : 0.2f;
        }
        else if (message.Contains("后悔") || message.Contains("遗憾") || message.Contains("当初") ||
                 message.Contains("如果") || message.Contains("要是"))
        {
            emotionalState["regret"] = emotionalState.ContainsKey("regret") ?
                Mathf.Min(emotionalState["regret"] + 0.2f, 1.0f) : 0.2f;
        }

        // 情绪衰减（模拟情绪随时间平复）
        List<string> keys = new List<string>(emotionalState.Keys);
        foreach (string key in keys)
        {
            emotionalState[key] = Mathf.Max(emotionalState[key] - 0.03f, 0);  // 缓慢衰减，给足耐心
            if (emotionalState[key] <= 0)
                emotionalState.Remove(key);
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
                        case "sadness": emotionName = "悲伤/委屈"; break;
                        case "happiness": emotionName = "开心"; break;
                        case "anger": emotionName = "愤怒"; break;
                        case "confusion": emotionName = "迷茫"; break;
                        case "anxiety": emotionName = "焦虑/内耗/不安"; break;
                        case "regret": emotionName = "后悔/遗憾"; break;
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
            temperature = 0.8f,  // 适中的温度，温婉但不失温度
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
                    yield return StartCoroutine(TypeWriterEffect(npcText, "（歉然地笑了笑）年纪大了，有时候思绪会飘远。你能再说一遍吗？"));
                }
            }
            else
            {
                Debug.LogError("API请求失败: " + request.error);
                Debug.LogError("响应内容: " + request.downloadHandler.text);
                yield return StartCoroutine(TypeWriterEffect(npcText, "（轻轻按了按眉心）海边的信号总是不太稳定。没关系，我们等一会儿。"));
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