using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Indicator of displaying stars (for a current level)
// The level number is calculated by searching in parent objects a LevelButton component 
public class ScoreStarFromMemory : MonoBehaviour {
	
	Image image;
    SpriteRenderer srenderer;
	
	public Sprite oneStar; // Image of one star
	public Sprite twoStar; // Image of two star
	public Sprite threeStar; // Image of three star
    public int level;

    void Awake() {
        image = GetComponent<Image>();
        srenderer = GetComponent<SpriteRenderer>();
	}
	
	void OnEnable () {
        Resresh();
    }

    public void Resresh() {
        Sprite result = null;
		switch (HowManyStars ()) {
			case 1: result = oneStar; break;
			case 2: result = twoStar; break;
			case 3: result = threeStar; break;
		}
        if (image) {
            image.enabled = result != null;
            image.sprite = result;
        }
        if (srenderer)
            srenderer.sprite = result;
    }

	public int HowManyStars () {
        return HowManyStars(level);
	}

	public static int HowManyStars (int level) {
		if (!Level.all.ContainsKey(level)) return 0;

		int numberOfStars = 0;

        int bestScore = ProfileAssistant.main.local_profile.GetScore(level);
		if (bestScore > Level.all[level].firstStarScore)
			numberOfStars ++;
		if (bestScore > Level.all[level].secondStarScore)
			numberOfStars ++;
		if (bestScore > Level.all[level].thirdStarScore)
			numberOfStars ++;
		
		return numberOfStars;
	}

}
