using System;
using System.Collections.Generic;

[Serializable]
public class CustomAudioEntry
{
    public string displayName;
    public string fileName;
    public string audioType;
}

[Serializable]
public class PlayerData
{
    public string username;
    public string password;
    public string gender;
    public int age;
    public long registerTime;
    public List<DiaryEntry> diaries = new List<DiaryEntry>();
    public List<CustomAudioEntry> customAudios = new List<CustomAudioEntry>();

    // ========== 宠物系统新增 ==========
    public int gold;
    public List<PetSaveData> pets = new List<PetSaveData>();
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public int lastSelectedPetId = -1;
    public int appearedPetId = -1;
    public int dailyTouchCount = 0;
    public string lastTouchDate = "";
}