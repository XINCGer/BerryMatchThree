using UnityEngine;
using System.Collections;

public class ScreenOrientationMask : MonoBehaviour {

    public ScreenOrientation screenOrientation;

    void Awake() {
        UIAssistant.onScreenResize += UpdateContent;
    }

	void OnEnable () {
        UpdateContent();
	}
	
	void UpdateContent () {
        bool landscape = Screen.width > Screen.height;
        bool visible = (screenOrientation == ScreenOrientation.Portrait && !landscape) || (screenOrientation == ScreenOrientation.Landscape && landscape);
        foreach (Transform child in transform)
            child.gameObject.SetActive(visible);
	}

    public enum ScreenOrientation {Portrait, Landscape};
}
