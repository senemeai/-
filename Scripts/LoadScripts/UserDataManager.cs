using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    private string dataPath;
    private List<PlayerData> playerList = new List<PlayerData>();
    private const string FILE_NAME = "playerData.json";
    private const string REMEMBER_KEY = "LastLoginUsername";
    private const string CURRENT_USER_KEY = "CurrentLoggedInUser";
    private readonly string encryptKey = "JieYouBar2024";

    public string CurrentLoggedInUser { get; private set; }

    private void Awake()
    {
        Instance = this;
        dataPath = Path.Combine(Application.persistentDataPath, FILE_NAME);
        LoadData();
        CurrentLoggedInUser = PlayerPrefs.GetString(CURRENT_USER_KEY, "");
    }

    // ================== 登录注册 ==================

    public bool IsUsernameExists(string username)
    {
        return playerList.Exists(u => u.username == username);
    }

    public bool ValidateLogin(string username, string password)
    {
        string encryptedPwd = XorEncrypt(password);
        return playerList.Exists(u => u.username == username && u.password == encryptedPwd);
    }

    public bool RegisterUser(string username, string password, string gender, int age)
    {
        if (IsUsernameExists(username)) return false;

        PlayerData newPlayer = new PlayerData
        {
            username = username,
            password = XorEncrypt(password),
            gender = gender,
            age = age,
            registerTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            diaries = new List<DiaryEntry>(),
            customAudios = new List<CustomAudioEntry>(),
            gold = 100, // 初始金币
            pets = new List<PetSaveData>
            {
                new PetSaveData { petId = 1, customName = "", affection = 0 },
                new PetSaveData { petId = 2, customName = "", affection = 0 },
                new PetSaveData { petId = 3, customName = "", affection = 0 },
                new PetSaveData { petId = 4, customName = "", affection = 0 },
                new PetSaveData { petId = 5, customName = "", affection = 0 }
            },
            inventory = new List<InventoryItem>(),
            lastSelectedPetId = 1,
            appearedPetId = -1,
            dailyTouchCount = 0,
            lastTouchDate = ""
        };

        playerList.Add(newPlayer);
        SaveData();
        return true;
    }

    public void RememberUsername(string username)
    {
        PlayerPrefs.SetString(REMEMBER_KEY, username);
        PlayerPrefs.Save();
    }

    public string GetRememberedUsername()
    {
        return PlayerPrefs.GetString(REMEMBER_KEY, "");
    }

    public void ClearRememberedUsername()
    {
        PlayerPrefs.DeleteKey(REMEMBER_KEY);
        PlayerPrefs.Save();
    }

    public PlayerData GetPlayer(string username)
    {
        var player = playerList.Find(u => u.username == username);
        if (player != null) FixPlayerData(player);  // ← 新增这行
        return player;
    }

    // 新增：修复旧存档缺失的宠物数据
    private void FixPlayerData(PlayerData player)
    {
        if (player.pets == null || player.pets.Count == 0)
        {
            player.pets = new List<PetSaveData>
            {
                new PetSaveData { petId = 1, customName = "", affection = 0 },
                new PetSaveData { petId = 2, customName = "", affection = 0 },
                new PetSaveData { petId = 3, customName = "", affection = 0 },
                new PetSaveData { petId = 4, customName = "", affection = 0 },
                new PetSaveData { petId = 5, customName = "", affection = 0 }
            };
            SaveData();
        }
        if (player.inventory == null) player.inventory = new List<InventoryItem>();
        if (player.customAudios == null) player.customAudios = new List<CustomAudioEntry>();
        if (player.diaries == null) player.diaries = new List<DiaryEntry>();
    }

    // ================== 当前用户 ==================

    public void SetCurrentUser(string username)
    {
        CurrentLoggedInUser = username;
        PlayerPrefs.SetString(CURRENT_USER_KEY, username);
        PlayerPrefs.Save();
    }

    public void ClearCurrentUser()
    {
        CurrentLoggedInUser = "";
        PlayerPrefs.DeleteKey(CURRENT_USER_KEY);
        PlayerPrefs.Save();
    }

    // ================== 修改用户信息 ==================

    public bool UpdatePlayerInfo(string oldUsername, string newUsername, string gender, int age, string newPassword)
    {
        PlayerData player = playerList.Find(p => p.username == oldUsername);
        if (player == null) return false;
        if (newUsername != oldUsername && IsUsernameExists(newUsername)) return false;

        player.username = newUsername;
        player.gender = gender;
        player.age = age;
        if (!string.IsNullOrEmpty(newPassword)) player.password = XorEncrypt(newPassword);

        SaveData();
        if (newUsername != oldUsername)
        {
            CurrentLoggedInUser = newUsername;
            RememberUsername(newUsername);
        }
        return true;
    }

    // ================== 自定义音频 ==================

    public void AddCustomAudio(string username, CustomAudioEntry entry)
    {
        PlayerData player = playerList.Find(p => p.username == username);
        if (player == null) return;
        if (player.customAudios == null) player.customAudios = new List<CustomAudioEntry>();
        player.customAudios.Add(entry);
        SaveData();
    }

    // ================== 日记管理 ==================

    public List<DiaryEntry> GetCurrentUserDiaries()
    {
        PlayerData player = playerList.Find(p => p.username == CurrentLoggedInUser);
        return player?.diaries ?? new List<DiaryEntry>();
    }

    public bool AddDiary(DiaryEntry diary)
    {
        PlayerData player = playerList.Find(p => p.username == CurrentLoggedInUser);
        if (player == null) return false;
        if (player.diaries == null) player.diaries = new List<DiaryEntry>();
        player.diaries.Add(diary);
        SaveData();
        return true;
    }

    public bool UpdateDiary(string diaryId, DiaryEntry newDiary)
    {
        PlayerData player = playerList.Find(p => p.username == CurrentLoggedInUser);
        if (player == null || player.diaries == null) return false;
        int index = player.diaries.FindIndex(d => d.id == diaryId);
        if (index < 0) return false;
        newDiary.id = diaryId;
        player.diaries[index] = newDiary;
        SaveData();
        return true;
    }

    public bool DeleteDiary(string diaryId)
    {
        PlayerData player = playerList.Find(p => p.username == CurrentLoggedInUser);
        if (player == null || player.diaries == null) return false;
        int index = player.diaries.FindIndex(d => d.id == diaryId);
        if (index < 0) return false;
        player.diaries.RemoveAt(index);
        SaveData();
        return true;
    }

    // ================== 宠物系统：金币 ==================

    public int GetGold()
    {
        var player = GetPlayer(CurrentLoggedInUser);
        return player?.gold ?? 0;
    }

    public void AddGold(int amount)
    {
        var player = GetPlayer(CurrentLoggedInUser);
        if (player == null) return;
        player.gold += amount;
        SaveData();
    }

    public bool SpendGold(int amount)
    {
        var player = GetPlayer(CurrentLoggedInUser);
        if (player == null) return false;
        if (player.gold < amount) return false;
        player.gold -= amount;
        SaveData();
        return true;
    }

    // ================== 宠物系统：宠物存档 ==================

    public PetSaveData GetPetSaveData(int petId)
    {
        var player = GetPlayer(CurrentLoggedInUser);
        if (player == null || player.pets == null) return null;
        var pet = player.pets.Find(p => p.petId == petId);
        if (pet == null)
        {
            pet = new PetSaveData { petId = petId, customName = "", affection = 0 };
            player.pets.Add(pet);
            SaveData();
        }
        return pet;
    }

    public void SetPetCustomName(int petId, string name)
    {
        var pet = GetPetSaveData(petId);
        if (pet != null) { pet.customName = name; SaveData(); }
    }

    public void AddPetAffection(int petId, int amount)
    {
        var pet = GetPetSaveData(petId);
        if (pet != null) { pet.affection += amount; SaveData(); }
    }

    public int GetLastSelectedPetId()
    {
        var player = GetPlayer(CurrentLoggedInUser);
        return player?.lastSelectedPetId ?? -1;
    }

    public void SetLastSelectedPetId(int id)
    {
        var player = GetPlayer(CurrentLoggedInUser);
        if (player != null) { player.lastSelectedPetId = id; SaveData(); }
    }

    public int GetAppearedPetId()
    {
        var player = GetPlayer(CurrentLoggedInUser);
        return player?.appearedPetId ?? -1;
    }

    public void SetAppearedPetId(int id)
    {
        var player = GetPlayer(CurrentLoggedInUser);
        if (player != null) { player.appearedPetId = id; SaveData(); }
    }

    // ================== 宠物系统：背包 ==================

    public List<InventoryItem> GetInventory()
    {
        var player = GetPlayer(CurrentLoggedInUser);
        return player?.inventory ?? new List<InventoryItem>();
    }

    public int GetItemCount(int itemId)
    {
        var player = GetPlayer(CurrentLoggedInUser);
        if (player?.inventory == null) return 0;
        var item = player.inventory.Find(i => i.itemId == itemId);
        return item?.count ?? 0;
    }

    public void AddItem(int itemId, int count)
    {
        var player = GetPlayer(CurrentLoggedInUser);
        if (player == null) return;
        if (player.inventory == null) player.inventory = new List<InventoryItem>();
        var item = player.inventory.Find(i => i.itemId == itemId);
        if (item == null) player.inventory.Add(new InventoryItem { itemId = itemId, count = count });
        else item.count += count;
        SaveData();
    }

    public bool RemoveItem(int itemId, int count)
    {
        var player = GetPlayer(CurrentLoggedInUser);
        if (player?.inventory == null) return false;
        var item = player.inventory.Find(i => i.itemId == itemId);
        if (item == null || item.count < count) return false;
        item.count -= count;
        if (item.count <= 0) player.inventory.Remove(item);
        SaveData();
        return true;
    }

    // ================== 宠物系统：抚摸上限 ==================

    public bool AddDailyTouchCount()
    {
        var player = GetPlayer(CurrentLoggedInUser);
        if (player == null) return false;

        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (player.lastTouchDate != today)
        {
            player.lastTouchDate = today;
            player.dailyTouchCount = 0;
        }

        if (player.dailyTouchCount >= 30) return false;

        player.dailyTouchCount++;
        SaveData();
        return true;
    }

    // ================== 数据持久化 ==================

    private void LoadData()
    {
        if (!File.Exists(dataPath))
        {
            playerList = new List<PlayerData>();
            return;
        }
        try
        {
            string encryptedJson = File.ReadAllText(dataPath);
            string json = XorDecrypt(encryptedJson);
            PlayerDataWrapper wrapper = JsonUtility.FromJson<PlayerDataWrapper>(json);
            playerList = wrapper.players ?? new List<PlayerData>();
        }
        catch
        {
            playerList = new List<PlayerData>();
        }
    }

    private void SaveData()
    {
        PlayerDataWrapper wrapper = new PlayerDataWrapper { players = playerList };
        string json = JsonUtility.ToJson(wrapper, true);
        string encryptedJson = XorEncrypt(json);
        try { File.WriteAllText(dataPath, encryptedJson); }
        catch (Exception e) { Debug.LogError("保存失败: " + e.Message); }
    }

    private string XorEncrypt(string input)
    {
        char[] output = new char[input.Length];
        for (int i = 0; i < input.Length; i++)
            output[i] = (char)(input[i] ^ encryptKey[i % encryptKey.Length]);
        return new string(output);
    }

    private string XorDecrypt(string input) { return XorEncrypt(input); }
}

[Serializable]
public class PlayerDataWrapper
{
    public List<PlayerData> players;
}