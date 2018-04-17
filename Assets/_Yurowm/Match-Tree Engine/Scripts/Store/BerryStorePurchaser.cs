using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BerryStorePurchaser : MonoBehaviour {

    public Text price;
    public Button purchaseButton;
    public int seedsCount;
    public string goodId;
    public int goodCount;

    void Start() {
        if (purchaseButton) purchaseButton.onClick.AddListener(Purchase);
        ItemCounter.refresh += OnEnable;
    }

    void OnEnable() {
        Refresh();
    }

    void Refresh() {
        if (price) price.text = seedsCount.ToString();
        if (purchaseButton)
            purchaseButton.gameObject.SetActive((ProfileAssistant.main.local_profile["seed"] >= seedsCount) || UIAssistant.main.GetCurrentPage() != "Store");
    }

    public void Purchase() {
        BerryStoreAssistant.main.Purchase(seedsCount, goodId, goodCount);
    }
}
