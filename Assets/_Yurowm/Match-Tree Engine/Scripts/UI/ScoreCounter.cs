using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// current score indicator 
[RequireComponent (typeof (Text))]
public class ScoreCounter : MonoBehaviour {
	
	
	Text label;

	float target = 0;
	float current = 0;
	
	void  Awake (){
		label = GetComponent<Text> ();
	} 

	void OnEnable () {
		current = SessionAssistant.main.score;
	}

	void  Update (){
		target = SessionAssistant.main.score;
		current = Mathf.MoveTowards (current, target, Time.unscaledDeltaTime * LevelProfile.main.thirdStarScore * 0.3f);
		label.text = Mathf.RoundToInt(current).ToString();
	}
}