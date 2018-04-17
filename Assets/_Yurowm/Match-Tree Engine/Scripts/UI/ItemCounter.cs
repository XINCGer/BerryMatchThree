using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Сценарий отображения элемента баланса в окружающей среде Soomla
[RequireComponent (typeof (Text))]
public class ItemCounter : MonoBehaviour {

	Text label;
	public string itemID; // Item ID
    public static System.Action refresh = delegate {};


	void Awake () {
		label = GetComponent<Text> ();
        refresh += Refresh;
	}
	
	void OnEnable () {
		Refresh (); // Updating when counter is activated
	}

	// Refreshing couter function
	public void Refresh() {
        if (!label)
            return;
        if (ProfileAssistant.main.local_profile != null)
            label.text = ProfileAssistant.main.local_profile[itemID].ToString();
        else
            label.text = "0";
	}

	// Refreshing all counters function
	public static void RefreshAll() {
        refresh.Invoke();
	}
}