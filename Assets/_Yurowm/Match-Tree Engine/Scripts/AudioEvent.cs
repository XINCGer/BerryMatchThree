using UnityEngine;
using System.Collections;

public class AudioEvent : MonoBehaviour {

	// Use this for initialization
	public void SoundShot (string clip) {
        AudioAssistant.Shot(clip);
	}
}
