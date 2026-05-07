using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DiaryItemUI : MonoBehaviour
{
    public TextMeshProUGUI textDate;
    public TextMeshProUGUI textPreview;
    public Button btnOpen;
    public Button btnEdit;
    public Button btnDelete;

    private DiaryEntry diaryData;
    private Action<DiaryEntry> onOpen;
    private Action<DiaryEntry> onEdit;
    private Action<DiaryEntry> onDelete;

    public void Setup(DiaryEntry diary, Action<DiaryEntry> openCallback,
                      Action<DiaryEntry> editCallback, Action<DiaryEntry> deleteCallback)
    {
        diaryData = diary;
        onOpen = openCallback;
        onEdit = editCallback;
        onDelete = deleteCallback;

        // 显示日期时间
        textDate.text = $"{diary.date.Substring(5)} {diary.time}";

        // 显示预览（前20字）
        string preview = diary.content.Length > 20
            ? diary.content.Substring(0, 20) + "..."
            : diary.content;
        textPreview.text = string.IsNullOrEmpty(preview) ? "（无内容）" : preview;

        // 绑定按钮
        btnOpen.onClick.RemoveAllListeners();
        btnOpen.onClick.AddListener(() => onOpen?.Invoke(diaryData));

        btnEdit.onClick.RemoveAllListeners();
        btnEdit.onClick.AddListener(() => onEdit?.Invoke(diaryData));

        btnDelete.onClick.RemoveAllListeners();
        btnDelete.onClick.AddListener(() => onDelete?.Invoke(diaryData));
    }
}