using UnityEngine;

[CreateAssetMenu(fileName = "PetConfig", menuName = "Game/PetConfig")]
public class PetConfig : ScriptableObject
{
    public int petId;
    public string petName;
    [TextArea(3, 5)] public string description;
    public string age;
    public string gender;
    public Sprite headIcon;
    public Sprite fullBody;
    public RuntimeAnimatorController animator;
}