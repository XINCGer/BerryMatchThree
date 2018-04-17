using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Class panel boosters
// Displays only the booster buttons that are relevant for the current limitation mode
[RequireComponent (typeof (GridLayoutGroup))]
public class BoosterList : MonoBehaviour {

	List<KeyValuePair<Limitation, Transform>> limitationPairs = new List<KeyValuePair<Limitation, Transform>>();

	void Awake () {
		Initialize ();
	}

	void Initialize ()
	{
		BoosterButton button;
		foreach (Transform booster in transform) {
			button = booster.GetComponent<BoosterButton>();
			foreach (Limitation target in button.limitationMask) {
				limitationPairs.Add(new KeyValuePair<Limitation, Transform>(target, booster));
			}
		}
	}

	void OnEnable () {
		Refresh ();
	}

	void Refresh ()
	{
		if (LevelProfile.main == null) return;
		foreach (Transform booster in transform)
			booster.gameObject.SetActive(false);
		foreach (KeyValuePair<Limitation, Transform> pair in limitationPairs) {
			if (pair.Key == LevelProfile.main.limitation)
				pair.Value.gameObject.SetActive(true);
		}
	}
}
