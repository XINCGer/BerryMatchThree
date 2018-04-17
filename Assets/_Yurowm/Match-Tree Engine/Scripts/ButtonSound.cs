using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (Button))]
public class ButtonSound : MonoBehaviour {

    

	// Use this for initialization
	void Start () {
        GetComponent<Button>().onClick.AddListener(OnClick);
	}
	
	// Update is called once per frame
	void OnClick () {
        AudioAssistant.Shot("UIButton");
	}
}
