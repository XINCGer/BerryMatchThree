using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// The script is responsible for the logic of the progress bar of game score
[RequireComponent (typeof (Slider))]
public class ScoreBar : MonoBehaviour {

    public static System.Action<StarType> onStarGet = delegate{};

	Slider slider;

	float target = 0;
	float current = 0;

	void Awake () {
		slider = GetComponent<Slider> ();
	}

	void OnEnable () {
		if (SessionAssistant.main == null) return;
		current = SessionAssistant.main.score;
	}

	void Update () {
		target = Mathf.Min(SessionAssistant.main.score, LevelProfile.main.thirdStarScore);
		current = Mathf.MoveTowards (current, target, Time.unscaledDeltaTime * LevelProfile.main.thirdStarScore * 0.3f);
		slider.value = current / LevelProfile.main.thirdStarScore;
        if (SessionAssistant.main.stars < 1 && current >= LevelProfile.main.firstStarScore) {
            SessionAssistant.main.stars = 1;
            onStarGet.Invoke(StarType.First);
        }
        if (SessionAssistant.main.stars < 2 && current >= LevelProfile.main.secondStarScore) {
            SessionAssistant.main.stars = 2;
            onStarGet.Invoke(StarType.Second);
        }
        if (SessionAssistant.main.stars < 3 && current >= LevelProfile.main.thirdStarScore) {
            SessionAssistant.main.stars = 3;
            onStarGet.Invoke(StarType.Third);
        }
	}
}
