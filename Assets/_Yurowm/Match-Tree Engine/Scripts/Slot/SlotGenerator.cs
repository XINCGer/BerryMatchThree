using UnityEngine;
using System.Collections;
using Berry.Utils;

// Slot which generates new simple chips.
[RequireComponent (typeof (Slot))]
[RequireComponent (typeof (Slot))]
public class SlotGenerator : MonoBehaviour {

	public Slot slot;
	public Chip chip;

	float lastTime = -10;
	float delay = 0.15f; // delay between the generations
	
	void  Awake (){
		slot = GetComponent<Slot>();
        slot.generator = true;
	}
	
	void  Update (){
        if (!SessionAssistant.main.enabled) return;
		
		if (slot.chip) return; // Generation is impossible, if slot already contains chip
		
		if (slot.block) return; // Generation is impossible, if the slot is blocked

        if (Chip.gravityBlockers.Count > 0) return;

		if (lastTime + delay > Time.time) return; // limit of frequency generation
		lastTime = Time.time;

        Vector3 spawnOffset = new Vector3(
            Utils.SideOffsetX(Utils.MirrorSide(slot.slotGravity.gravityDirection)),
            Utils.SideOffsetY(Utils.MirrorSide(slot.slotGravity.gravityDirection)),
            0) * 0.4f;

        if (LevelProfile.main.target == FieldTarget.SugarDrop && SessionAssistant.main.creatingSugarDropsCount > 0) {
            if (SugarChip.live_count == 0 || SessionAssistant.main.GetResource() <= 0.4f + 0.6f * SessionAssistant.main.creatingSugarDropsCount / LevelProfile.main.targetSugarDropsCount) {
                SessionAssistant.main.creatingSugarDropsCount--;
                FieldAssistant.main.GetSugarChip(slot.coord, transform.position + spawnOffset); // creating new sugar chip
                return;
            }
        }

		if (Random.value > LevelProfile.main.stonePortion)
            FieldAssistant.main.GetNewSimpleChip(slot.coord, transform.position + spawnOffset); // creating new chip
		else
            FieldAssistant.main.GetNewStone(slot.coord, transform.position + spawnOffset); // creating new stone
	}
}