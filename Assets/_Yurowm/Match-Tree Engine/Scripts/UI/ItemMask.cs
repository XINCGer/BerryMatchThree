using UnityEngine;
using System.Collections;

// The script is responsible for displaying child objects, depending on the item balance in Soomla environment
public class ItemMask : MonoBehaviour {

	public string itemID; // Item ID in Soomla environment
	public ComparisonOperator mustBe; // Logical operand
	public int value = 1; // Comparison value

	public enum ComparisonOperator {Less, Greater, Equal, EqualLess, EqualGreater};

	void Start () {
		Refresh ();
        ItemCounter.refresh += Refresh;
	}

	void OnEnable () {
		Refresh (); // Updating when object is activated
	}

	// Refreshing
	public void Refresh () {
		int balance = 0;
        if (ProfileAssistant.main.local_profile != null)
            balance = ProfileAssistant.main.local_profile[itemID];        
		bool result = false;
		switch (mustBe) {
		case ComparisonOperator.Less: result = balance < value; break;
		case ComparisonOperator.Greater: result = balance > value; break;
		case ComparisonOperator.EqualLess: result = balance <= value; break;
		case ComparisonOperator.EqualGreater: result = balance >= value; break;
		case ComparisonOperator.Equal: result = balance == value; break;
		}
		SetVisible (result);
	}

	// Scenario of display / hide child objects
	void SetVisible (bool v) {
		foreach (Transform t in transform) {
			t.gameObject.SetActive(v);
		}
	}
}
