using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Class button activation booster
[RequireComponent (typeof (Button))]
public class BoosterButton : MonoBehaviour {

   

	// booster type
	// Page - showing a special page "value" booster
	// Message - send a message "value" itself. It assumes the presence of a script with the logic of the booster
	public enum BoosterButtonType {Page, Message};
    // Booster ID from Soomla Storage system 
    public static string boosterSelectedId = "";
    public string logic_content;

    public string boosterItemId;
	public BoosterButtonType type;
	public string value;
	// Mask of displaying booster depending on the limitation mode
	public Limitation[] limitationMask;

	Button button;
	
	void Awake () {
        SendMessage("BoosterInitialize", SendMessageOptions.DontRequireReceiver);
		button = GetComponent<Button> ();
        foreach (ItemCounter counter in GetComponentsInChildren<ItemCounter>(true)) {
            counter.itemID = boosterItemId;
            counter.Refresh();
        }
        foreach (ItemMask mask in GetComponentsInChildren<ItemMask>()) {
            mask.itemID = boosterItemId;
            mask.Refresh();
        }
		button.onClick.AddListener(() => {
			OnClick();
		});
	}

	
	void OnClick () {
        if (!SessionAssistant.main.CanIWait())
            return;
		if (ProfileAssistant.main.local_profile[boosterItemId] == 0) {
            boosterSelectedId = boosterItemId;
            UIAssistant.main.ShowPage("Store");
			return;
		}
		if (type == BoosterButtonType.Message)
			SendMessage(value, SendMessageOptions.DontRequireReceiver);
        if (type == BoosterButtonType.Page) {
            boosterSelectedId = boosterItemId;
            UIAssistant.main.ShowPage("Booster");
            ContentAssistant.main.GetItem(logic_content, Vector3.zero);
        }
	}
}
