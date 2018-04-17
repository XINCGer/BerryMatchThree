using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

// "Cut" booster
// This object must be in the UI-panel of the booster. During activation (OnEnable) it turn a special mode of interaction with chips (ControlAssistant ignored)
[RequireComponent (typeof (BoosterButton))]
public class BoosterCut : IBoosterLogic {

    public Animation spoon;

	// Coroutine of special control mode
	public override IEnumerator Logic () {
        spoon.gameObject.SetActive(false);

		yield return StartCoroutine (Utils.WaitFor (SessionAssistant.main.CanIWait, 0.1f));

		Slot target = null;
		while (true) {
			if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
				target = ControlAssistant.main.GetSlotFromTouch();
            if (target != null && (!target.chip ||  target.chip.chipType != "Sugar")) {

                spoon.transform.position = target.transform.position;
                spoon.gameObject.SetActive(true);
                spoon.Play();

                CPanel.uiAnimation++;

                yield return new WaitForSeconds(0.91f);

                ProfileAssistant.main.local_profile["spoon"] --;
                ItemCounter.RefreshAll();
               
				FieldAssistant.main.BlockCrush(target.coord, false);
				FieldAssistant.main.JellyCrush(target.coord);
				
                SessionAssistant.main.EventCounter();

                if (target.chip) {
                    target.chip.jamType = Jam.GetType(target);
                    target.chip.DestroyChip();
                }

                while (spoon.isPlaying)
                    yield return 0;

                spoon.gameObject.SetActive(false);

                CPanel.uiAnimation--;

                break;
			}
			yield return 0;
		}

        UIAssistant.main.ShowPage("Field");
	}
}
