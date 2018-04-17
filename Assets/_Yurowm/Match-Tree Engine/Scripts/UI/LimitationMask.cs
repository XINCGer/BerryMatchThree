using UnityEngine;
using System.Collections;

// The script responsible for displaying child objects, depending on the limitation mode
public class LimitationMask : MonoBehaviour {

	public Limitation[] visibleMask; // List of limitation modes in which child objects will be displayed

	void OnEnable () {
		Refresh (); // Updating when object is activated
	}

	// Refreshing
	void Refresh() {
		if (LevelProfile.main == null) {
			SetVisible (false);
			return;
		}
		bool v = false;
		foreach (Limitation t in visibleMask) {
			if (t == LevelProfile.main.limitation) {
				v = true;
				break;
			}
		}
		SetVisible (v);
	}

	// Scenario of display / hide child objects
	void SetVisible (bool v) {
		foreach (Transform t in transform) {
			t.gameObject.SetActive(v);
		}
	}
}
