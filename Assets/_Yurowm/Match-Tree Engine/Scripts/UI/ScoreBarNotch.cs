using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Script is responsible for displaying notches (goals for stars) on the progress bar of game score.
[RequireComponent (typeof (RectTransform))]
public class ScoreBarNotch : MonoBehaviour {

	RectTransform rect;
	public StarType star;

	void Awake() {
		rect = GetComponent<RectTransform> ();
		}

	void OnEnable () {
		if (LevelProfile.main == null)
						return;
		float value = 0;
		float max = LevelProfile.main.thirdStarScore;
		switch (star) {
			case StarType.First: value = LevelProfile.main.firstStarScore; break;
			case StarType.Second: value = LevelProfile.main.secondStarScore; break;
			case StarType.Third: value = LevelProfile.main.thirdStarScore; break;
		}
		value = value / max;
		Vector2 pos = rect.anchoredPosition;
		pos.x = value * ((RectTransform)rect.parent).rect.width - rect.rect.width;
		rect.anchoredPosition = pos;
	}
}

public enum StarType {First, Second, Third};