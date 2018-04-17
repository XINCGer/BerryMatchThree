using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Berry.Utils;

// The component responsible for the transfer of chips from one slot to another in accordance with physics
[RequireComponent (typeof (Slot))]
public class SlotGravity : MonoBehaviour {

	public Slot slot;

    public Side gravityDirection = Side.Null;
    public Side fallingDirection = Side.Null;

	// No shadow - is a direct path from the slot up to the slot with a component SlotGenerator. Towards must have slots (without blocks and wall)
	// This concept is very important for the proper physics chips
	public bool shadow;
	
	void  Awake (){
		slot = GetComponent<Slot>();
	}


	// Update shadows at all slots (for example, after the blocks destruction)
	public static void  Reshading () { 
        foreach (SlotGravity sg in GameObject.FindObjectsOfType<SlotGravity>())
            sg.shadow = true;

        Slot slot;
        List<Slot> stock = new List<Slot>();
        List<SlotGenerator> generator = new List<SlotGenerator>(GameObject.FindObjectsOfType<SlotGenerator>());
        // Gravity shading
        foreach (SlotGenerator sgen in generator) {
            slot = sgen.slot;
            stock.Clear();
            while (slot && !slot.block && slot.slotGravity.shadow && !stock.Contains(slot)) {
                slot.slotGravity.shadow = false;
                stock.Add(slot);
                slot = slot[slot.slotGravity.gravityDirection];
            }
            sgen.slot.slotGravity.shadow = false;
        }

        if (GameObject.FindObjectsOfType<SlotTeleport>().Length > 0) {
            // Teleport shading
            foreach (SlotGenerator sgen in generator) {
                slot = sgen.slot;
                stock.Clear(); 
                while (slot && !slot.block && !stock.Contains(slot)) {
                    slot.slotGravity.shadow = false;
                    stock.Add(slot);
                    if (slot.slotTeleport) 
                        slot = slot.slotTeleport.target;
                    else
                        slot = slot[slot.slotGravity.gravityDirection];
                }
                sgen.slot.slotGravity.shadow = false;
            }
        }


        //foreach (SlotGravity s in GameObject.FindObjectsOfType<SlotGravity>())
        //    ScoreBubble.Bubbling(s.shadow ? 1 : 0, s.transform, 0);

        //Debug.Break();

	}
	
	void  Update (){
        if (slot && slot.chip && !slot.chip.busy)
            GravityReaction();
	}

	// Gravity iteration
	public void  GravityReaction (){		
        if (!SessionAssistant.main.isPlaying) return; 
		
        if (!slot || !slot.chip) return;

        if (Chip.gravityBlockers.Count > 0) return;

		if (transform.position != slot.chip.transform.position) return; // Work is possible only if the chip is physically clearly in the slot

        if (!slot[gravityDirection] || slot[gravityDirection].block)
            return; // Work is possible only if there is another bottom slot

        if (slot[gravityDirection] && slot[gravityDirection].chip && slot[gravityDirection].chip.busy)
            return;

        // provided that bottom neighbor doesn't contains chip, give him our chip
        if (!slot[gravityDirection].chip) {
            slot[gravityDirection].chip = slot.chip;
			GravityReaction();
			return;
		} 

		// Otherwise, we try to give it to their neighbors from the bottom-left and bottom-right side
		if (Random.value > 0.5f) { // Direction priority is random
			SlideLeft();
			SlideRight();
		} else {
			SlideRight();
			SlideLeft();	
		}
	}



    void SlideLeft() {
        Side cw45side = Utils.RotateSide(gravityDirection, 1);
        Side cw90side = Utils.RotateSide(gravityDirection, 2);

        if (slot[cw45side] // target slot must exist
            && !slot[cw45side].block // target slot must be unbloked
            && ((slot[gravityDirection] && slot[gravityDirection][cw90side]) || (slot[cw90side] && slot[cw90side][gravityDirection])) // target slot should have a no-diagonal path that is either left->down or down->left
            && !slot[cw45side].chip // target slot should not have a chip
            && slot[cw45side].GetShadow() // target slot must have shadow otherwise it will be easier to fill it with a generator on top
            && !slot[cw45side].GetChipShadow()) { // target slot should not be shaded by another chip, otherwise it will be easier to fill it with this chip
            slot[cw45side].chip = slot.chip; // transfer chip to target slot
		}
	}

    void SlideRight() {
        Side ccw45side = Utils.RotateSide(gravityDirection, -1);
        Side ccw90side = Utils.RotateSide(gravityDirection, -2);

        if (slot[ccw45side] // target slot must exist
            && !slot[ccw45side].block // target slot must contain gravity
            && ((slot[gravityDirection] && slot[gravityDirection][ccw90side]) || (slot[ccw90side] && slot[ccw90side][gravityDirection])) // target slot should have a no-diagonal path that is either right->down or down->right
            && !slot[ccw45side].chip // target slot should not have a chip
            && slot[ccw45side].GetShadow() // target slot must have shadow otherwise it will be easier to fill it with a generator on top
            && !slot[ccw45side].GetChipShadow()) {// target slot should not be shaded by another chip, otherwise it will be easier to fill it with this chip
			slot[ccw45side].chip = slot.chip; // transfer chip to target slot
		}
	}
}