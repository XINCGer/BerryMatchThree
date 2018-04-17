using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BerryStorePurchaserIAP : MonoBehaviour {

    public Text price;
    public Button purchaseButton;
    public string sku;
    public string goodId;
    public int goodCount;

	void Awake () {
        purchaseButton.onClick.AddListener(Purchase);
	}

    void OnEnable() {
        Refresh();
    }

	void Purchase () {
        BerryStoreAssistant.main.PurchaseIAP(sku, goodId, goodCount);
	}

    public void Refresh() {
        price.text = "N/A";
        if (BerryStoreAssistant.main.marketItemPrices.ContainsKey(sku))
            price.text = BerryStoreAssistant.main.marketItemPrices[sku];
    }
}
