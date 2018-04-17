using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorTargetBar : MonoBehaviour {

	// indexes:
	// 0 : red item;
	// 1 : greenItem;
	// 2 : blueItem;
	// 3 : yellowItem;
	// 4 : purpleItem;
	// 5 : orangeItem;
	public Transform[] items;
	public Text[] counters;
	ModeMask modeMask;
	int z;
	bool active = true;

	void Awake() {
		modeMask = GetComponent<ModeMask> ();
	}

	void OnEnable () {
		if (!modeMask) return;
		active = false;
		foreach (FieldTarget target in modeMask.visibleMask) {
			if (LevelProfile.main.target == target) {
				active = true;
				return;
			}
		}
		for (int i = 0; i < 6; i++) {
			items[i].gameObject.SetActive(false);
		}
	}



	void Update () {
		if (!active) return;
		for (int i = 0; i < 6; i++) {
			z = SessionAssistant.main.countOfEachTargetCount[i];
			counters[i].text = z.ToString();
			items[i].gameObject.SetActive(z > 0);
		}
	}
}
