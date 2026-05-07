using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MonthGroupUI : MonoBehaviour
{
    public Button btnMonthHeader;
    public TextMeshProUGUI textMonth;
    public Image iconArrow;
    public Transform diaryItemContainer;  // 指向 DiaryListScrollView/Viewport/Content
    public ScrollRect diaryListScrollRect;  // 指向 DiaryListScrollView（控制显示/隐藏）

    private bool isExpanded = true;
    private string monthKey;

    public void Setup(string yearMonth, Action<string> onToggleCallback)
    {
        monthKey = yearMonth;
        string[] parts = yearMonth.Split('-');
        textMonth.text = $"{parts[0]}年{int.Parse(parts[1])}月 ▼";

        btnMonthHeader.onClick.RemoveAllListeners();
        btnMonthHeader.onClick.AddListener(() =>
        {
            isExpanded = !isExpanded;
            diaryListScrollRect.gameObject.SetActive(isExpanded);

            textMonth.text = isExpanded
                ? $"{parts[0]}年{int.Parse(parts[1])}月 ▼"
                : $"{parts[0]}年{int.Parse(parts[1])}月 ▶";
            iconArrow.rectTransform.localRotation = isExpanded
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 0, -90);

            onToggleCallback?.Invoke(monthKey);
        });
    }

    public Transform GetContainer()
    {
        return diaryItemContainer;
    }
}