using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Effect of displaying of score points after destruction of chips
public class ScoreBubble : MonoBehaviour {

	public Text text;
	Animation animationc;
	public int colorID = -1;
	public int score = 0;
	static Color[] colors = {
        new Color(1f, 0.3f, 0.3f),
		new Color(0.3f, 1f, 0.3f),
		new Color(0.3f, 0.8f, 1f),
		new Color(1f, 1f, 0.3f),
		new Color(1f, 0.3f, 1f),
		new Color(1f, 0.6f, 0.3f)};
	
	void  Awake (){
		animationc = GetComponent<Animation>();
	}
	
	void  Start (){
		text.text = score.ToString ();
        Gradient gradient = text.GetComponent<Gradient>();
        if (colorID >= 0 && colorID < colors.Length)
            gradient.EndColor = colors[colorID];

        transform.SetParent(Slot.folder);
        animationc.Play();
		}

    void Update() {
        if (!animationc.isPlaying)
            Destroy(gameObject);
    }

	public static void Bubbling (int score, Transform trans, int id) {
		ScoreBubble bubble = ContentAssistant.main.GetItem<ScoreBubble>("ScoreBubble", trans.position);
		bubble.score = score;
		bubble.colorID = id;
	}

    public static void Bubbling(int score, Transform trans) {
        ScoreBubble bubble = ContentAssistant.main.GetItem<ScoreBubble>("ScoreBubble", trans.position);
        bubble.score = score;
    }
}