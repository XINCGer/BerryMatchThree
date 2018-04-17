using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

// "Shake" booster
// This object should be connected to a button component BoosterButton, which will send the message "BoosterActivate"
public class BoosterShake : MonoBehaviour {
	
	public static bool busy = false;
    BoosterButton booster;

	// Booster activation
	public void BoosterActivate () {
		if (busy) return;
		StartCoroutine (CreatingBombs ());
	}
	
	//Coroutine of booster working
	IEnumerator CreatingBombs () {
		busy = true;
		yield return StartCoroutine (Utils.WaitFor(SessionAssistant.main.CanIWait, 0.1f));
		yield return StartCoroutine (SessionAssistant.main.Shuffle (true));
        ProfileAssistant.main.local_profile["shaker"]--;
        ItemCounter.RefreshAll();
		busy = false;
	}
}
