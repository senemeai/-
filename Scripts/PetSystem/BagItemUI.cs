using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BagItemUI : MonoBehaviour
{
    public int itemId;
    public Button btn;              // ¡û ÐÂÔö
    public Image bgSelected;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textCount;
    public Image iconItem;

    public void Setup(ShopConfig config, int count, System.Action<int> onClick)
    {
        itemId = config.itemId;
        textName.text = config.itemName;
        textCount.text = "x" + count;
        if (config.icon != null) iconItem.sprite = config.icon;
        if (btn != null) btn.onClick.AddListener(() => onClick(itemId));
    }

    public void SetSelected(bool selected)
    {
        bgSelected.gameObject.SetActive(selected);
    }
}