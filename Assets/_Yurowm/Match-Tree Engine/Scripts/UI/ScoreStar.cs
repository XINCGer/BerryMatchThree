using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// received stars indicator (for current level and current score)
[RequireComponent (typeof (Image))]
public class ScoreStar : MonoBehaviour {

	Image image;
    Animation anim;
	float lastUpdate = 0;
	bool filled = false;

	public Sprite fullStar; // Image of received star
	public Sprite emptyStar; // Image of unreceived star
	public StarType starType; // displaying star type - first, second, third
	public bool fromCurrentScore = true;

	void Awake () {
		image = GetComponent<Image> ();
        anim = GetComponent<Animation>();
	}

	void OnEnable () {
		lastUpdate = 0;
		filled = false;
        if (emptyStar)
            image.sprite = emptyStar;
        else
            image.enabled = false;
	}

	void Update () {
		if (filled) return;
		if (lastUpdate + 0.5f > Time.unscaledTime) return;
		lastUpdate = Time.unscaledTime;
		float target = 0;
		switch (starType) {
			case StarType.First: target = LevelProfile.main.firstStarScore; break;
			case StarType.Second: target = LevelProfile.main.secondStarScore; break;
			case StarType.Third: target = LevelProfile.main.thirdStarScore; break;
		}

        if (fromCurrentScore)
            filled = target <= SessionAssistant.main.score;
        else
            filled = target <= ProfileAssistant.main.local_profile.GetScore(LevelProfile.main.level);

        if (anim)
            anim.enabled = filled;

        if (filled) {
            image.enabled = true;
			image.sprite = fullStar;
        }
	}
}
