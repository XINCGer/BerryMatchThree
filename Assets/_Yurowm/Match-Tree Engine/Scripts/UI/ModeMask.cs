using UnityEngine;
using System.Collections;

// The script responsible for displaying child objects, depending on the game mode
public class ModeMask : MonoBehaviour {

	public FieldTarget[] visibleMask; // List of game modes in which child objects will be displayed

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
		foreach (FieldTarget t in visibleMask) {
			if (t == LevelProfile.main.target) {
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
