using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public int itemId;
    public Button btn;              // ¡û ÐÂÔö
    public Image bgSelected;
    public TextMeshProUGUI textName;
    public Image iconProduct;

    public void Setup(ShopConfig config, System.Action<int> onClick)
    {
        itemId = config.itemId;
        textName.text = config.itemName;
        if (config.icon != null) iconProduct.sprite = config.icon;
        if (btn != null) btn.onClick.AddListener(() => onClick(itemId));
    }

    public void SetSelected(bool selected)
    {
        bgSelected.gameObject.SetActive(selected);
    }
}