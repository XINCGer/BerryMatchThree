using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Berry.Utils;

// "Switch" Booster
// This object must be in the UI-panel of the booster. During activation (OnEnable) it turn a special mode of interaction with chips
public class BoosterSwitch : IBoosterLogic {

    public Animation hand;

	// Coroutine of special control mode
	public override IEnumerator Logic ()
	{
        hand.gameObject.SetActive(false);
		yield return StartCoroutine (Utils.WaitFor (SessionAssistant.main.CanIWait, 0.1f));

        Chip chipA = null;
        Chip chipB = null;
        Side side = Side.Null;
        System.Action<Chip, Side> fu = (Chip c, Side s) => {
            if (c.slot && c.slot[s]) {
                chipA = c;
                chipB = c.slot[s].chip;
                side = s;
            }
        };
        
        ControlAssistant.swap = fu;
        
        while (chipA == null || chipB == null)
            yield return 0;
        
        ProfileAssistant.main.local_profile["hand"]--;
        ControlAssistant.swap = Chip.Swap;

        Vector3 rotation = new Vector3();
        switch (side) {
            case Side.Bottom:
                rotation.z = 0;
                break;
            case Side.Left:
                rotation.z = -90;
                break;
            case Side.Top:
                rotation.z = 180;
                break;
            case Side.Right:
                rotation.z = 90;
                break;
        }

        hand.gameObject.SetActive(true);
        hand.transform.position = chipA.slot.transform.position;
        hand.transform.eulerAngles = rotation;
        hand.Play();

        yield return new WaitForSeconds(0.5f);

        SessionAssistant.main.SwapByPlayer(chipA, chipB, true);
        SessionAssistant.main.swapEvent--;
		SessionAssistant.main.movesCount ++;		

        while (hand.isPlaying)
            yield return 0;
		
        hand.gameObject.SetActive(false);
        UIAssistant.main.ShowPage("Field");
	}

    public override void Disable() {
        ControlAssistant.swap = Chip.Swap;
    }
}
