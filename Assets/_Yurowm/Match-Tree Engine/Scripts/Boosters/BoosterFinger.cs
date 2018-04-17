using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

// "Finger" booster
// This object must be in the UI-panel of the booster. During activation (OnEnable) it turn a special mode of interaction with chips (ControlAssistant ignored)
public class BoosterFinger : IBoosterLogic {

    public Animation finger;

	// Coroutine of special control mode
    public override IEnumerator Logic() {
        finger.gameObject.SetActive(false);
		yield return StartCoroutine (Utils.WaitFor (SessionAssistant.main.CanIWait, 0.1f));

        Slot target = null;
        while (true) {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
                target = ControlAssistant.main.GetSlotFromTouch();
            if (target && target.chip && target.chip.chipType == "SimpleChip") {

                AudioAssistant.Shot("MagicFinger");

                finger.gameObject.SetActive(true);
                finger.transform.position = target.transform.position;
                finger.Play();

                yield return new WaitForSeconds(2);

                ProfileAssistant.main.local_profile["finger"]--;

                float prob = Random.value;
                if (prob < 0.35f)
                    FieldAssistant.main.AddPowerup(target.coord, "SimpleBomb");
                else if (prob < 0.7f)
                    FieldAssistant.main.AddPowerup(target.coord, "CrossBomb");
                else if (prob < 0.9f)
                    FieldAssistant.main.AddPowerup(target.coord, "ColorBomb");
                else
                    FieldAssistant.main.AddPowerup(target.coord, "RainbowHeart");
                SessionAssistant.main.EventCounter();
                break;
            }
            yield return 0;
        }

        while (finger.isPlaying)
            yield return 0;

        finger.gameObject.SetActive(false);

        UIAssistant.main.ShowPage("Field");
	}
}
