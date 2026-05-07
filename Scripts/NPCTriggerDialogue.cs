using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCTriggerDialogue : MonoBehaviour
{
    [Header("气泡UI组件")]
    public GameObject bubblePanel;           // 整个气泡面板
    public TextMeshProUGUI bubbleText;       // 气泡文字
    public Button btnChat;                   // "聊"按钮
    public Button btnCancel;                 // "不聊"按钮

    [Header("对话框")]
    public GameObject bossDialogueCanvas;    // 当前NPC的Canvas
    public MonoBehaviour dialogueScript;     // 当前NPC的对话脚本（万能适配）

    [Header("触发设置")]
    public string playerTag = "Player";      // 玩家标签
    public float autoHideDelay = 5f;         // 无操作自动隐藏时间（秒）

    [Header("对话内容")]
    [TextArea(1, 3)]
    public string greetingMessage = "";      // 问候语（在Unity里自己填）
    public string cancelMessage = "";        // 取消消息（在Unity里自己填）

    // 内部状态
    private bool isPlayerInRange = false;
    private float hideTimer = 0f;
    private GameObject currentPlayer;
    private bool isShowingCancelMessage = false;
    private string originalBubbleText;

    void Start()
    {
        if (bubbleText != null)
        {
            originalBubbleText = greetingMessage;
            bubbleText.text = greetingMessage;
        }

        if (btnChat != null)
            btnChat.onClick.AddListener(OnClickChat);
        if (btnCancel != null)
            btnCancel.onClick.AddListener(OnClickCancel);

        SetupCollider();
    }

    void SetupCollider()
    {
        BoxCollider2D col2D = GetComponent<BoxCollider2D>();
        if (col2D != null)
        {
            col2D.isTrigger = true;
            return;
        }

        BoxCollider2D newCol = gameObject.AddComponent<BoxCollider2D>();
        newCol.isTrigger = true;
        newCol.size = new Vector2(1.5f, 1.5f);
        Debug.Log($"[{gameObject.name}] 自动添加了BoxCollider2D");
    }

    void Update()
    {
        if (!isPlayerInRange) return;

        if (!isShowingCancelMessage && hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                HideBubble();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            currentPlayer = other.gameObject;
            isPlayerInRange = true;
            ShowBubble();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            currentPlayer = null;
            HideBubble();

            if (bossDialogueCanvas != null && bossDialogueCanvas.activeSelf)
            {
                bossDialogueCanvas.SetActive(false);
            }
        }
    }

    void ShowBubble()
    {
        if (bubblePanel == null) return;

        isShowingCancelMessage = false;
        bubbleText.text = originalBubbleText;

        bubblePanel.SetActive(true);
        hideTimer = autoHideDelay;
    }

    void HideBubble()
    {
        if (bubblePanel != null)
            bubblePanel.SetActive(false);
        hideTimer = 0;
        isShowingCancelMessage = false;
    }

    void OnClickChat()
    {
        HideBubble();

        if (bossDialogueCanvas != null)
        {
            bossDialogueCanvas.SetActive(true);

            if (dialogueScript != null)
            {
                var method = dialogueScript.GetType().GetMethod("StartDialogue");
                if (method != null)
                {
                    method.Invoke(dialogueScript, null);
                }
            }
        }

        LockPlayerMovement(true);
    }

    void OnClickCancel()
    {
        if (!isShowingCancelMessage)
        {
            isShowingCancelMessage = true;
            bubbleText.text = cancelMessage;
            bubblePanel.SetActive(true);
            hideTimer = 3f;
        }
        else
        {
            HideBubble();
        }
    }

    void LockPlayerMovement(bool locked)
    {
        if (currentPlayer == null) return;

        // 根据你的玩家控制器类型取消注释
        // var playerMovement = currentPlayer.GetComponent<玩家移动脚本>();
        // if (playerMovement != null) playerMovement.enabled = !locked;

        // var rb = currentPlayer.GetComponent<Rigidbody2D>();
        // if (rb != null) rb.velocity = Vector2.zero;
    }

    public void OnDialogueClosed()
    {
        LockPlayerMovement(false);
    }

    void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.size);
        }
    }
}