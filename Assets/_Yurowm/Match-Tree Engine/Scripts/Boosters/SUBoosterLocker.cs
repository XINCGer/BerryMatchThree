using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class SUBoosterLocker : MonoBehaviour {

    public int unlockFrom = 5;
    Image icon;
    Sprite unlockedIcon;
    public Sprite lockedIcon;
    Button button;

    SUBoosterButton sub;

    GameObject selected;
    GameObject unselected;

	// Use this for initialization
	void Start () {
        icon = transform.Find("Icon").GetComponent<Image>();
        unlockedIcon = icon.sprite;
        button = GetComponent<Button>();

        sub = GetComponent<SUBoosterButton>();

        selected = transform.Find("Selected").gameObject;
        unselected = transform.Find("Unselected").gameObject;

        OnEnable();
    }
	
	// Update is called once per frame
	void OnEnable () {
        if (icon == null)
            return;
	    if (LevelProfile.main.level >= unlockFrom) 
            icon.sprite = unlockedIcon;
        else 
            icon.sprite = lockedIcon;

        if (LevelProfile.main.level < unlockFrom && SUBoosterButton.bag.Contains(sub.boosterItemId))
            button.onClick.Invoke();

        button.enabled = icon == LevelProfile.main.level >= unlockFrom;
        selected.SetActive(LevelProfile.main.level >= unlockFrom && SUBoosterButton.bag.Contains(sub.boosterItemId));
        unselected.SetActive(LevelProfile.main.level >= unlockFrom && !SUBoosterButton.bag.Contains(sub.boosterItemId));

    }
}
