using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Button))]
public class StarLevelButton : MonoBehaviour {

	public void OnClick () {
		if (CPanel.uiAnimation > 0) return;
        FieldAssistant.main.StartLevel();
	}
}
