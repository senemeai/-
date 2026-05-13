using UnityEngine;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "Game/ShopConfig")]
public class ShopConfig : ScriptableObject
{
    public int itemId;
    public string itemName;
    [TextArea(3, 5)] public string description;
    public Sprite icon;
    public int price;
    [TextArea(2, 3)] public string effectDesc;
    public int affectionValue; // 投喂后增加的好感度
}