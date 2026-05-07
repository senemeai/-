using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string username;
    public string password;
    public string gender;
    public int age;
    public long registerTime;
    public List<DiaryEntry> diaries = new List<DiaryEntry>();
}