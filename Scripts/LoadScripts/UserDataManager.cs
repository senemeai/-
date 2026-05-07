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

        // 从 PlayerPrefs 恢复当前登录用户（跨场景传递用户名）
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
            diaries = new List<DiaryEntry>()
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
        return playerList.Find(u => u.username == username);
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

    // ================== 设置：修改用户信息 ==================

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

    // ================== 日记功能 ==================

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