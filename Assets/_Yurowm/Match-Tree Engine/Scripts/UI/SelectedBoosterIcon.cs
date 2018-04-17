using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Image))]
public class SelectedBoosterIcon : MonoBehaviour {

    Image image;
    public Sprite[] icons;
    public string[] names;

	void Awake () {
        image = GetComponent<Image>();
	}
	
	void OnEnable () {
        Refresh();
	}

    public void Refresh() {
        string booster = DinaLabel.words["BoosterSelectedName"].Invoke();
        Debug.Log("Refresh " + booster);
        for (int i = 0; i < names.Length; i++) {
            if (booster == names[i]) {
                image.sprite = icons[i];
                return;
            }
        }

    }
    
}
