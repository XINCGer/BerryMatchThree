using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections;
using System.Collections.Generic;
using System;

// Implementation of Unity IAP
public class BerryStoreAssistant : MonoBehaviour, IStoreListener, INeedLocalization {

    static IStoreController store_controller;
    static IExtensionProvider store_extension_provider;

    public List<ItemInfo> items = new List<ItemInfo>();
    public ItemInfo GetItemByID(string id) {
        return items.Find(x => x.id == id);
    }

    public List<IAP> iaps = new List<IAP>();
    public IAP GetIAPByID(string id) {
        return iaps.Find(x => x.id == id);
    }

	public static BerryStoreAssistant main;
	public Dictionary<string, string> marketItemPrices = new Dictionary<string, string>();

    Action iap_reward = delegate{};

	void Awake () {
		main = this;
        DebugPanel.AddDelegate("Add some seeds", () => {
            main.Purchase(0, "seed", 100);
        });
	}

	void Start () {
        if (store_controller==null)
            InitializePurchasing();;
	}

    void InitializePurchasing() {
        if (IsInitialized())
            return;

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (IAP iap in iaps)
            builder.AddProduct(iap.id, ProductType.Consumable, new IDs() {
                { iap.sku, AppleAppStore.Name},
                { iap.sku, GooglePlay.Name}});

        UnityPurchasing.Initialize(this, builder);        
    }

    bool IsInitialized() {
        return store_controller != null && store_extension_provider != null;
    }

    void UpdatePrices() {
        if (store_controller == null)
            return;

        foreach (IAP iap in iaps) {
            Product product = store_controller.products.WithID(iap.id);
            if (product == null)
                continue;

            marketItemPrices.Add(iap.id, product.metadata.localizedPriceString);
        }
    }

    // Function item purchase
    public void Purchase(int seedsCount, string goodId, int goodCount) {
        if (ProfileAssistant.main.local_profile["seed"] < seedsCount) {
            UIAssistant.main.ShowPage("Store");
            return;
        }
        ProfileAssistant.main.local_profile["seed"] -= seedsCount;
        ProfileAssistant.main.local_profile[goodId] += goodCount;
        ProfileAssistant.main.SaveUserInventory();
        ItemCounter.RefreshAll();
        AudioAssistant.Shot("Buy");

    }

	// Function item purchase
	public void PurchaseIAP (string id, string goodId, int goodCount)
	{
        IAP iap = GetIAPByID(id);
        if (iap != null) {
            iap_reward = () => {Purchase(0, goodId, goodCount);};
            BuyProductID(iap.id);
        }
    }

    void BuyProductID(string id) {
        try {
            if (IsInitialized()) {
                Product product = store_controller.products.WithID(id);
                if (product != null && product.availableToPurchase) {
                    Debug.Log("Purchasing product asychronously:'" + product.definition.id + "'");// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
                    store_controller.InitiatePurchase(product);
                } else
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
        } else
                Debug.Log("BuyProductID FAIL. Not initialized.");
        }
        catch (Exception e) {
            Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
        }
    }

    #region Unity IAP Implementation
    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.LogError("Unity IAP Purchasing initializing is failed: " + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
        iap_reward.Invoke();
        iap_reward = null;
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p) {
        Debug.Log("Purchase failed: " + p);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        Debug.Log("Unity IAP Purchasing is initialized");

        store_controller = controller;
        store_extension_provider = extensions;

        UpdatePrices();
    }
    #endregion

    public List<string> RequriedLocalizationKeys() {
        List<string> result = new List<string>();
        foreach (ItemInfo item in items) {
            result.Add("item_" + item.id + "_description");
            result.Add("item_" + item.id + "_name");
        }
        return result;
    }

    [System.Serializable]
    public class ItemInfo {
        public string name;
        public string id;
        public string localization_description { get { return "item_" + id + "_description"; } }
        public string localization_name { get { return "item_" + id + "_name"; } }
    }

    [System.Serializable]
    public class IAP {
        public string id;
        public string sku;
    }
}
