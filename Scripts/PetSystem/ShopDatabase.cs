using System.Collections.Generic;
using UnityEngine;

public class ShopDatabase : MonoBehaviour
{
    public static ShopDatabase Instance { get; private set; }
    public List<ShopConfig> allItems = new List<ShopConfig>();

    void Awake() { Instance = this; }

    public ShopConfig GetItem(int id) { return allItems.Find(i => i.itemId == id); }
}