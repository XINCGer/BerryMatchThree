using UnityEngine;
using System.Collections;
using Berry.Utils;

public class SlotTeleport : MonoBehaviour {

	public Slot target;
	public Slot slot;

	public int2 target_postion = null;

	float lastTime = -10;
	float delay = 0.15f; // delay between the generations
	
	void  Start (){
		slot = GetComponent<Slot>();
        slot.slotTeleport = this;
	}

	public void Initialize () {
        if (!enabled) return;
		int2 position = target_postion;

        target = Slot.GetSlot(position);
        if (target) {
            target.teleportTarget = true;
        } else {
            Destroy(this);
        }
	}

	void  Update (){
		if (!target) return; // Teleport is possible only if target is exist
		
		if (!slot.chip) return; // Teleport is possible only if slot contains chip

        if (slot.chip.busy) return; // If chip can't be moved, then it can't be teleported

		if (target.chip) return; // Teleport is impossible if target slot already contains chip
				
		if (slot.block) return; // Teleport is impossible, if the slot is blocked
		if (target.block) return; // Teleport is impossible, if the target slot is blocked

		if (slot.chip.transform.position != slot.transform.position) return;
		
		if (lastTime + delay > Time.time) return; // limit of frequency generation
		lastTime = Time.time;

		AnimationAssistant.main.TeleportChip (slot.chip, target);
	}
}
