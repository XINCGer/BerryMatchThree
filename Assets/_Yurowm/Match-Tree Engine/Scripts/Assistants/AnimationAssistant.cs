using UnityEngine;
using System.Collections;

public class AnimationAssistant : MonoBehaviour {
	// This script is responsible for procedural animations in the game. Such as change of place 2 chips and the effect of the explosion.
	
	public static AnimationAssistant main; // Main instance. Need for quick access to functions.
	void  Awake (){
		main = this;
	}

	// Function of creating of explosion effect
	public void  Explode (Vector3 center, float radius, float force){
		Chip[] chips = GameObject.FindObjectsOfType<Chip>();
		Vector3 impuls;
		foreach(Chip chip in chips) {
			if ((chip.transform.position - center).magnitude > radius) continue;
			impuls = (chip.transform.position - center) * force;
			impuls *= Mathf.Pow((radius - (chip.transform.position - center).magnitude) / radius, 2);
			chip.impulse += impuls;
		}
	}

	public void TeleportChip(Chip chip, Slot target) {
		StartCoroutine (TeleportChipRoutine (chip, target));
	}

	IEnumerator TeleportChipRoutine (Chip chip, Slot target) {
		if (!chip.slot) yield break;
        if (chip.destroying) yield break;
        if (target.chip || target.block) yield break;

        Vector3 scale_target = Vector3.zero;
        target.chip = chip;
        chip.busy = true;
    
        scale_target.z = 1;
        while (chip.transform.localScale.x != scale_target.x) {
            chip.transform.localScale = Vector3.MoveTowards(chip.transform.localScale, scale_target, Time.deltaTime * 20);
            yield return 0;
            if (!chip) yield break;
        }

        chip.transform.localPosition = Vector3.zero;
        scale_target.x = 1;
        scale_target.y = 1;
        while (chip.transform.localScale.x != scale_target.x) {
            chip.transform.localScale = Vector3.MoveTowards(chip.transform.localScale, scale_target, Time.deltaTime * 20);
            yield return 0;
            if (!chip) yield break;
        }

        chip.busy = false;
    }

}