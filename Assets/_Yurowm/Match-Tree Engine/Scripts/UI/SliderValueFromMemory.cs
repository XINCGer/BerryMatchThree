using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (Slider))]
public class SliderValueFromMemory : MonoBehaviour {

    public string key = "";
    Slider slider;

	void Awake () {
        slider = GetComponent<Slider>();
	}
	
	void OnEnable () {
        slider.value = PlayerPrefs.GetFloat(key);
	}
}
