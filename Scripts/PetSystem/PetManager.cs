using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance { get; private set; }

    [Header("面板根节点")]
    public GameObject petCanvas;
    public GameObject panelMain;
    public GameObject bgDetail;
    public GameObject panelPetList;
    public GameObject panelShopList;
    public GameObject panelPetDetail;
    public GameObject panelShopDetail;
    public GameObject panelBag;
    public GameObject petOnStageObj;      // ← 新增：拖 PetOnStage 空物体
    public Image petOnStageImage;
    [Header("左侧列表")]
    public Transform contentPets;
    public Transform contentShop;
    public GameObject petItemPrefab;
    public GameObject shopItemPrefab;

    [Header("背包")]
    public Transform contentBag;
    public GameObject bagItemPrefab;

    [Header("宠物详情")]
    public Image petImage;
    public Animator petAnimator;
    public Animator petDetailAnimator;
    public TextMeshProUGUI textName;
    public TMP_InputField inputName;
    public TextMeshProUGUI textAffection;
    public TextMeshProUGUI textAge;
    public TextMeshProUGUI textGender;

    [Header("商品详情")]
    public Image productImage;
    public TextMeshProUGUI textProductName;
    public TextMeshProUGUI textDescription;
    public TextMeshProUGUI textPrice;

    [Header("按钮")]
    public Button btnPet;  // ← 新增
    public Button btnClose;
    public Button btnPetSelect;
    public Button btnShop;
    public Button btnEditName;
    public Button btnFeed;
    public Button btnPlay;
    public Button btnTouch;
    public Button btnAppear;
    public Button btnBuy;
    public Button btnConfirmFeed;
    public Button btnCancelBag;

    [Header("金币")]
    public TextMeshProUGUI textGoldCount;

    [Header("特效")]
    public GameObject evolutionEffect;
    public GameObject handTouchEffect;
    public GameObject playEffect;
    public GameObject feedEffect;
    public Transform floatTextRoot;
    public GameObject floatTextPrefab;

    private int currentPetId = -1;
    private int currentShopItemId = -1;
    private int selectedBagItemId = -1;
    private bool isEditingName = false;

    void Awake()
    {
        Instance = this;
        btnPet.onClick.AddListener(OnBtnPetClick);        // ← 新增
        btnClose.onClick.AddListener(ClosePetCanvas);
        btnPetSelect.onClick.AddListener(() => SwitchTab("pet"));
        btnShop.onClick.AddListener(() => SwitchTab("shop"));
        btnEditName.onClick.AddListener(OnEditName);
        btnFeed.onClick.AddListener(OnFeedClick);
        btnPlay.onClick.AddListener(OnPlayClick);
        btnTouch.onClick.AddListener(OnTouchClick);
        btnAppear.onClick.AddListener(OnAppearClick);
        btnBuy.onClick.AddListener(OnBuyClick);
        btnConfirmFeed.onClick.AddListener(OnConfirmFeed);
        btnCancelBag.onClick.AddListener(OnCancelBag);
    }
    void OnBtnPetClick()
    {
        if (!panelMain.activeSelf)
        {
            panelMain.SetActive(true);
            OpenPetCanvas();
        }
    }
    void OnEnable()
    {
        OpenPetCanvas();
    }

    public void OpenPetCanvas()
    {
        textGoldCount.text = UserDataManager.Instance.GetGold().ToString();
        SwitchTab("pet");

        int lastId = UserDataManager.Instance.GetLastSelectedPetId();
        if (lastId != -1) OnPetSelected(lastId);
    }

    void ClosePetCanvas()
    {
        // 只隐藏内容面板，不隐藏 PetCanvas（因为 Btn_Pet 在里面）
        panelMain.SetActive(false);
    }

    void SwitchTab(string tab)
    {
        if (tab == "pet")
        {
            panelPetList.SetActive(true);
            panelShopList.SetActive(false);
            bgDetail.SetActive(false);
            panelPetDetail.SetActive(false);
            panelShopDetail.SetActive(false);
            panelBag.SetActive(false);
            RefreshPetList();
        }
        else
        {
            panelPetList.SetActive(false);
            panelShopList.SetActive(true);
            bgDetail.SetActive(false);
            panelPetDetail.SetActive(false);
            panelShopDetail.SetActive(false);
            panelBag.SetActive(false);
            RefreshShopList();
        }
    }

    // ================== 宠物列表 ==================

    void RefreshPetList()
    {
        for (int i = contentPets.childCount - 1; i >= 0; i--)
            Destroy(contentPets.GetChild(i).gameObject);

        var player = UserDataManager.Instance.GetPlayer(UserDataManager.Instance.CurrentLoggedInUser);
        if (player == null || player.pets == null) return;

        foreach (var petSave in player.pets)
        {
            var config = PetDatabase.Instance.GetPet(petSave.petId);
            if (config == null) continue;

            GameObject item = Instantiate(petItemPrefab, contentPets);
            item.SetActive(true);
            item.GetComponent<PetItemUI>().Setup(config, OnPetSelected);
        }
    }

    void OnPetSelected(int petId)
    {
        currentPetId = petId;
        UserDataManager.Instance.SetLastSelectedPetId(petId);

        foreach (Transform child in contentPets)
        {
            var ui = child.GetComponent<PetItemUI>();
            if (ui != null) ui.SetSelected(ui.petId == petId);
        }

        bgDetail.SetActive(true);
        panelPetDetail.SetActive(true);
        panelShopDetail.SetActive(false);
        panelBag.SetActive(false);

        var config = PetDatabase.Instance.GetPet(petId);
        var save = UserDataManager.Instance.GetPetSaveData(petId);

        string displayName = string.IsNullOrEmpty(save.customName) ? config.petName : save.customName;
        textName.text = displayName;
        inputName.text = displayName;
        textAffection.text = save.affection.ToString();
        textAge.text = config.age;
        textGender.text = config.gender;

        petImage.sprite = config.fullBody;
        petImage.preserveAspect = true; // ← 保持原比例，绝不压缩变形

        // ← 根据原图等比例放大显示（这里放大 1.5 倍，你自己调）
        RectTransform rt = petImage.rectTransform;
        Vector2 originalSize = config.fullBody.rect.size;
        float displayScale = 2.5f;
        rt.sizeDelta = originalSize * displayScale;

        if (config.animator != null && petAnimator != null)

            petDetailAnimator.runtimeAnimatorController = config.animator;
    }

    // ================== 商城列表 ==================

    void RefreshShopList()
    {
        for (int i = contentShop.childCount - 1; i >= 0; i--)
            Destroy(contentShop.GetChild(i).gameObject);

        foreach (var config in ShopDatabase.Instance.allItems)
        {
            GameObject item = Instantiate(shopItemPrefab, contentShop);
            item.SetActive(true);
            item.GetComponent<ShopItemUI>().Setup(config, OnShopItemSelected);
        }
    }

    void OnShopItemSelected(int itemId)
    {
        currentShopItemId = itemId;

        foreach (Transform child in contentShop)
        {
            var ui = child.GetComponent<ShopItemUI>();
            if (ui != null) ui.SetSelected(ui.itemId == itemId);
        }

        bgDetail.SetActive(true);
        panelPetDetail.SetActive(false);
        panelShopDetail.SetActive(true);
        panelBag.SetActive(false);

        var config = ShopDatabase.Instance.GetItem(itemId);
        textProductName.text = config.itemName;
        textDescription.text = config.description;
        textPrice.text = config.price + " 金币";
        textPrice.color = new Color(1f, 0.84f, 0f);
        productImage.sprite = config.icon;
    }

    // ================== 改名 ==================

    void OnEditName()
    {
        if (!isEditingName)
        {
            isEditingName = true;
            textName.gameObject.SetActive(false);
            inputName.gameObject.SetActive(true);
            inputName.ActivateInputField();
        }
        else
        {
            string newName = inputName.text.Trim();
            if (!string.IsNullOrEmpty(newName) && currentPetId != -1)
            {
                UserDataManager.Instance.SetPetCustomName(currentPetId, newName);
                textName.text = newName;
            }
            isEditingName = false;
            textName.gameObject.SetActive(true);
            inputName.gameObject.SetActive(false);
        }
    }

    // ================== 投喂 & 背包 ==================

    void OnFeedClick()
    {
        if (currentPetId == -1) return;
        panelBag.SetActive(true);
        RefreshBagList();
    }

    void RefreshBagList()
    {
        for (int i = contentBag.childCount - 1; i >= 0; i--)
            Destroy(contentBag.GetChild(i).gameObject);

        selectedBagItemId = -1;
        var player = UserDataManager.Instance.GetPlayer(UserDataManager.Instance.CurrentLoggedInUser);
        if (player == null || player.inventory == null) return;

        foreach (var inv in player.inventory)
        {
            if (inv.count <= 0) continue;
            var config = ShopDatabase.Instance.GetItem(inv.itemId);
            if (config == null) continue;

            GameObject item = Instantiate(bagItemPrefab, contentBag);
            item.SetActive(true);
            item.GetComponent<BagItemUI>().Setup(config, inv.count, (id) =>
            {
                selectedBagItemId = id;
                foreach (Transform c in contentBag)
                {
                    var b = c.GetComponent<BagItemUI>();
                    if (b != null) b.SetSelected(b.itemId == id);
                }
            });
        }
    }

    void OnConfirmFeed()
    {
        if (selectedBagItemId == -1) return;

        var config = ShopDatabase.Instance.GetItem(selectedBagItemId);
        if (config == null) return;

        bool ok = UserDataManager.Instance.RemoveItem(selectedBagItemId, 1);
        if (!ok) return;

        UserDataManager.Instance.AddPetAffection(currentPetId, config.affectionValue);

        var save = UserDataManager.Instance.GetPetSaveData(currentPetId);
        textAffection.text = save.affection.ToString();

        panelBag.SetActive(false);
        PlayEffect("feed");
        ShowFloatText("+" + config.affectionValue + " 好感度", new Vector2(0, 50));
    }

    void OnCancelBag()
    {
        panelBag.SetActive(false);
    }

    // ================== 玩耍 & 抚摸 & 出场 ==================

    void OnPlayClick()
    {
        PlayEffect("play");
    }

    void OnTouchClick()
    {
        bool canReward = UserDataManager.Instance.AddDailyTouchCount();
        if (canReward)
        {
            UserDataManager.Instance.AddGold(5);
            textGoldCount.text = UserDataManager.Instance.GetGold().ToString();
            PlayEffect("touch");
            ShowFloatText("+5 金币", new Vector2(200, 200));
        }
        else
        {
            ShowFloatText("今日抚摸奖励已达上限", new Vector2(0, 100));
        }
    }

    void OnAppearClick()
    {
        if (currentPetId == -1) return;

        // 保存出场宠物到存档
        UserDataManager.Instance.SetAppearedPetId(currentPetId);

        // 关闭面板
        panelMain.SetActive(false);

        // 显示桌面宠物（只有这里才操作桌面图！）
        if (petOnStageObj != null)
        {
            petOnStageObj.SetActive(true);

            if (petOnStageImage != null)
            {
                var config = PetDatabase.Instance.GetPet(currentPetId);
                if (config != null)
                {
                    if (config.fullBody != null)
                    {
                        petOnStageImage.sprite = config.fullBody;
                        petOnStageImage.preserveAspect = true;
                    }
                    if (config.animator != null && petAnimator != null)
                    {
                        petAnimator.runtimeAnimatorController = config.animator;
                    }
                }
            }
        }
    }

    // ================== 购买 ==================

    void OnBuyClick()
    {
        var config = ShopDatabase.Instance.GetItem(currentShopItemId);
        if (config == null) return;

        bool ok = UserDataManager.Instance.SpendGold(config.price);
        if (ok)
        {
            UserDataManager.Instance.AddItem(currentShopItemId, 1);
            textGoldCount.text = UserDataManager.Instance.GetGold().ToString();
            ShowFloatText("购买成功", new Vector2(0, 0));
        }
        else
        {
            ShowFloatText("金币不足", new Vector2(0, 0));
        }
    }

    // ================== 特效 & 飘字 ==================

    void PlayEffect(string type)
    {
        switch (type)
        {
            case "evolution": evolutionEffect.SetActive(true); break;
            case "touch": handTouchEffect.SetActive(true); break;
            case "play": playEffect.SetActive(true); break;
            case "feed": feedEffect.SetActive(true); break;
        }
        StartCoroutine(HideEffectAfter(type, 1.5f));
    }

    IEnumerator HideEffectAfter(string type, float delay)
    {
        yield return new WaitForSeconds(delay);
        switch (type)
        {
            case "evolution": evolutionEffect.SetActive(false); break;
            case "touch": handTouchEffect.SetActive(false); break;
            case "play": playEffect.SetActive(false); break;
            case "feed": feedEffect.SetActive(false); break;
        }
    }

    void ShowFloatText(string text, Vector2 anchoredPos)
    {
        if (floatTextPrefab == null || floatTextRoot == null)
        {
            Debug.LogWarning("FloatTextPrefab 或 FloatTextRoot 未绑定，跳过飘字");
            return;
        }

        GameObject ft = Instantiate(floatTextPrefab, floatTextRoot);
        ft.SetActive(true);
        RectTransform rt = ft.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        TextMeshProUGUI tmp = ft.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        StartCoroutine(FloatTextAnim(ft));
    }

    IEnumerator FloatTextAnim(GameObject ft)
    {
        RectTransform rt = ft.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = ft.GetComponent<TextMeshProUGUI>();
        Vector3 start = rt.localPosition;
        float t = 0;
        while (t < 1.2f)
        {
            t += Time.deltaTime;
            rt.localPosition = start + Vector3.up * 50 * (t / 1.2f);
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1 - (t / 1.2f));
            yield return null;
        }
        Destroy(ft);
    }
}