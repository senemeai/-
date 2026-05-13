using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public class DiaryUIManager : MonoBehaviour
{
    [Header("日记主面板")]
    public GameObject panelDiary;
    public Button btnCloseDiary;
    public TextMeshProUGUI textDiaryCount;
    public Button btnWriteDiary;

    [Header("日记列表")]
    public Transform contentDiaryList;
    public GameObject prefabDiaryItem;

    [Header("日记详情")]
    public GameObject panelDiaryDetail;
    public Button btnCloseDetail;
    public TextMeshProUGUI textDetailTitle;
    public TextMeshProUGUI textDetailDate;
    public TextMeshProUGUI textDetailWeather;
    public TextMeshProUGUI textDetailMood;
    public TextMeshProUGUI textDetailContent;
    public Button btnBackToList;

    [Header("写日记面板")]
    public GameObject panelWriteDiary;
    public Button btnCloseWrite;
    public TextMeshProUGUI textAutoDate;
    public TMP_Dropdown dropdownWeather;
    public TMP_Dropdown dropdownMood;
    public TMP_InputField inputDiaryTitle;
    public TMP_InputField inputDiaryContent;
    public Button btnSaveDiary;
    public TextMeshProUGUI textSaveTips;

    [Header("删除确认")]
    public GameObject panelDeleteConfirm;
    public Button btnConfirmDelete;
    public Button btnCancelDelete;

    [Header("日记本按钮")]
    public Button btnDiaryBook;

    private readonly string[] weatherOptions = { "晴天", "多云", "阴", "小雨", "大雨", "小雪","大雪", "雾" };
    private readonly string[] moodOptions = { "开心", "平静", "难过", "愤怒", "焦虑", "幸福" ,"伤心"};

    private DiaryEntry currentEditingDiary = null;
    private DiaryEntry diaryToDelete = null;

    private void Start()
    {
        btnDiaryBook.onClick.AddListener(OpenDiaryPanel);
        btnCloseDiary.onClick.AddListener(CloseDiaryPanel);
        btnWriteDiary.onClick.AddListener(OpenWritePanel);
        btnCloseWrite.onClick.AddListener(OnCloseWriteClick);
        btnSaveDiary.onClick.AddListener(OnSaveDiary);
        btnCloseDetail.onClick.AddListener(CloseDetailPanel);
        btnBackToList.onClick.AddListener(CloseDetailPanel);
        btnConfirmDelete.onClick.AddListener(OnConfirmDelete);
        btnCancelDelete.onClick.AddListener(OnCancelDelete);

        dropdownWeather.ClearOptions();
        dropdownWeather.AddOptions(new List<string>(weatherOptions));

        dropdownMood.ClearOptions();
        dropdownMood.AddOptions(new List<string>(moodOptions));

        panelDiary.SetActive(false);
        panelDiaryDetail.SetActive(false);
        panelWriteDiary.SetActive(false);
        panelDeleteConfirm.SetActive(false);
        textSaveTips.gameObject.SetActive(false);
    }

    public void OpenDiaryPanel()
    {
        panelDiary.SetActive(true);
        RefreshDiaryList();
    }

    public void CloseDiaryPanel()
    {
        panelDiary.SetActive(false);
        panelDiaryDetail.SetActive(false);
        panelWriteDiary.SetActive(false);
        panelDeleteConfirm.SetActive(false);
    }

    private void RefreshDiaryList()
    {
        foreach (Transform child in contentDiaryList)
        {
            Destroy(child.gameObject);
        }

        List<DiaryEntry> diaries = UserDataManager.Instance.GetCurrentUserDiaries();
        textDiaryCount.text = $"共 {diaries.Count} 篇日记";

        if (diaries.Count == 0) return;

        diaries = diaries.OrderByDescending(d => d.timestamp).ToList();

        foreach (var diary in diaries)
        {
            GameObject itemObj = Instantiate(prefabDiaryItem, contentDiaryList);
            DiaryItemUI itemUI = itemObj.GetComponent<DiaryItemUI>();
            itemUI.Setup(diary, OnOpenDiary, OnEditDiary, OnDeleteDiary);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentDiaryList as RectTransform);
    }

    private void OnOpenDiary(DiaryEntry diary)
    {
        // 显示标题（标题为空时显示默认提示）
        textDetailTitle.text = string.IsNullOrEmpty(diary.title) ? "无标题" : diary.title;

        textDetailDate.text = $"{diary.date} {GetWeekDay(diary.date)} {diary.time}";
        textDetailWeather.text = GetWeatherIcon(diary.weather) + " " + diary.weather;
        textDetailMood.text = diary.mood;
        textDetailContent.text = diary.content;
        panelDiaryDetail.SetActive(true);
    }

    private void CloseDetailPanel()
    {
        panelDiaryDetail.SetActive(false);
    }

    private void OpenWritePanel()
    {
        currentEditingDiary = null;

        DateTime now = DateTime.Now;
        textAutoDate.text = now.ToString("yyyy-MM-dd HH:mm");

        dropdownWeather.value = 0;
        dropdownMood.value = 1;
        inputDiaryTitle.text = "";
        inputDiaryContent.text = "";
        textSaveTips.gameObject.SetActive(false);

        panelWriteDiary.SetActive(true);
    }

    private void OpenWritePanelForEdit(DiaryEntry diary)
    {
        currentEditingDiary = diary;

        textAutoDate.text = $"{diary.date} {diary.time}";

        int weatherIndex = Array.IndexOf(weatherOptions, diary.weather);
        dropdownWeather.value = weatherIndex >= 0 ? weatherIndex : 0;

        int moodIndex = Array.IndexOf(moodOptions, diary.mood);
        dropdownMood.value = moodIndex >= 0 ? moodIndex : 1;

        inputDiaryTitle.text = diary.title;
        inputDiaryContent.text = diary.content;
        textSaveTips.gameObject.SetActive(false);

        panelWriteDiary.SetActive(true);
    }

    private void OnCloseWriteClick()
    {
        panelWriteDiary.SetActive(false);
        currentEditingDiary = null;
    }

    private void OnSaveDiary()
    {
        string title = inputDiaryTitle.text.Trim();
        string content = inputDiaryContent.text.Trim();
        string weather = weatherOptions[dropdownWeather.value];
        string mood = moodOptions[dropdownMood.value];

        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(content))
        {
            ShowSaveTips("标题或内容至少填写一项", false);
            return;
        }

        DateTime now = DateTime.Now;

        if (currentEditingDiary == null)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = $"{now.Month}月{now.Day}日 {mood}日记";
            }

            DiaryEntry newDiary = new DiaryEntry
            {
                id = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                title = title,
                date = now.ToString("yyyy-MM-dd"),
                time = now.ToString("HH:mm"),
                weather = weather,
                mood = mood,
                content = content,
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
            };

            UserDataManager.Instance.AddDiary(newDiary);
            ShowSaveTips("保存成功！", true);
        }
        else
        {
            DiaryEntry updatedDiary = new DiaryEntry
            {
                title = string.IsNullOrEmpty(title) ? currentEditingDiary.title : title,
                date = currentEditingDiary.date,
                time = currentEditingDiary.time,
                weather = weather,
                mood = mood,
                content = content,
                timestamp = currentEditingDiary.timestamp
            };

            UserDataManager.Instance.UpdateDiary(currentEditingDiary.id, updatedDiary);
            ShowSaveTips("修改成功！", true);
        }

        Invoke(nameof(CloseWriteAndRefresh), 0.5f);
    }

    private void ShowSaveTips(string msg, bool isSuccess)
    {
        textSaveTips.text = msg;
        textSaveTips.color = isSuccess ? new Color(0, 1, 0) : new Color(1, 0.3f, 0);
        textSaveTips.gameObject.SetActive(true);
    }

    private void CloseWriteAndRefresh()
    {
        panelWriteDiary.SetActive(false);
        currentEditingDiary = null;
        RefreshDiaryList();
    }

    private void OnEditDiary(DiaryEntry diary)
    {
        OpenWritePanelForEdit(diary);
    }

    private void OnDeleteDiary(DiaryEntry diary)
    {
        diaryToDelete = diary;
        panelDeleteConfirm.SetActive(true);
    }

    private void OnConfirmDelete()
    {
        if (diaryToDelete != null)
        {
            UserDataManager.Instance.DeleteDiary(diaryToDelete.id);
            diaryToDelete = null;
        }
        panelDeleteConfirm.SetActive(false);
        RefreshDiaryList();
    }

    private void OnCancelDelete()
    {
        diaryToDelete = null;
        panelDeleteConfirm.SetActive(false);
    }

    private string GetWeekDay(string dateStr)
    {
        if (DateTime.TryParse(dateStr, out DateTime date))
        {
            string[] weekdays = { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            return weekdays[(int)date.DayOfWeek];
        }
        return "";
    }

    private string GetWeatherIcon(string weather)
    {
        switch (weather)
        {
            case "晴": return "晴天";
            case "多云": return "多云";
            case "阴": return "阴天";
            case "小雨": return "小雨";
            case "大雨": return "大雨";
            case "雪": return "学";
            case "雾": return "雾天";
            default: return "天气";
        }
    }

    private void OnDestroy()
    {
        btnDiaryBook.onClick.RemoveAllListeners();
        btnCloseDiary.onClick.RemoveAllListeners();
        btnWriteDiary.onClick.RemoveAllListeners();
        btnCloseWrite.onClick.RemoveAllListeners();
        btnSaveDiary.onClick.RemoveAllListeners();
        btnCloseDetail.onClick.RemoveAllListeners();
        btnBackToList.onClick.RemoveAllListeners();
        btnConfirmDelete.onClick.RemoveAllListeners();
        btnCancelDelete.onClick.RemoveAllListeners();
    }
}