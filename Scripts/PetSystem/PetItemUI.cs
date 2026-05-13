using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetItemUI : MonoBehaviour
{
    public int petId;
    public Button btn;              // ← 新增：把自身 Button 拖进来
    public Image bgSelected;
    public TextMeshProUGUI textName;
    public Image iconHead;

    public void Setup(PetConfig config, System.Action<int> onClick)
    {
        petId = config.petId;
        textName.text = config.petName;
        if (config.headIcon != null) iconHead.sprite = config.headIcon;
        if (btn != null) btn.onClick.AddListener(() => onClick(petId));
    }

    public void SetSelected(bool selected)
    {
        bgSelected.gameObject.SetActive(selected);
    }
}