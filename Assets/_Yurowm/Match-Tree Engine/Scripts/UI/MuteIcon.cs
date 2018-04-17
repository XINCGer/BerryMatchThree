using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Image))]
public class MuteIcon : MonoBehaviour {

    Image image;
    public Sprite on;
    public Sprite off;

	public void UpdateIcon () {
        if (!image)
            image = GetComponent<Image>();
        image.sprite = AudioAssistant.main.mute ? off : on;
	}
}
