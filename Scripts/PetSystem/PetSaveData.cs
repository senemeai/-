using System;

[Serializable]
public class PetSaveData
{
    public int petId;
    public string customName; // 空表示使用默认名
    public int affection;
}