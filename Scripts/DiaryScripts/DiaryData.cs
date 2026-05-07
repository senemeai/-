using System;
using System.Collections.Generic;

[Serializable]
public class DiaryEntry
{
    public string id;
    public string title;
    public string date;        // 格式：2025-04-28
    public string time;        // 格式：14:30
    public string weather;
    public string mood;
    public string content;
    public long timestamp;     // 创建时间戳，用于排序和唯一标识
}

[Serializable]
public class NPCDialogueRecord
{
    public string npcName;
    public string date;
    public string time;
    public List<string> playerInputs = new List<string>();
    public long timestamp;
}