using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Image))]
public class FieldBackground : MonoBehaviour {

    Image image;

    public static Sprite background;

	void Awake () {
        image = GetComponent<Image>();
        UIAssistant.onScreenResize += LoadBackground;
	}

    void OnEnable() {
        LoadBackground();
    }

    void LoadBackground() {
        if (background != null)
            image.sprite = background;
       
        Texture2D texture = image.sprite.texture;
        float texture_ratio = 1f * texture.width / texture.height;
        float screen_ratio = 1f * Screen.width / Screen.height;

        RectTransform rect = transform as RectTransform;

        if (texture_ratio > screen_ratio) {
            rect.offsetMin = new Vector2(-600, 0);
            rect.offsetMax = new Vector2(600, 0);            
        } else {
            rect.offsetMin = new Vector2(0, -600);
            rect.offsetMax = new Vector2(0, 600);
        }
    }
}
