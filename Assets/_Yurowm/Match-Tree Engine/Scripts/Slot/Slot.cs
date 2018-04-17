using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

// Base class for slots
public class Slot : MonoBehaviour {

    public static Dictionary<int2, Slot> all = new Dictionary<int2, Slot>();

    public bool generator = false;
    public bool teleportTarget = false;
    public Jelly jelly; // Jelly for this slot
    public Jam jam;
    public IBlock block; // Block for this slot

    // Position of this slot
    public int2 coord = new int2();
    public int x { get { return coord.x;} }
	public int y { get { return coord.y;} }

	public Slot this[Side index] { // access to neighby slots on the index
		get {
			return nearSlot[index];
		}
	}

    public Dictionary<Side, Slot> nearSlot = new Dictionary<Side, Slot> (); // Nearby slots dictionary
	public Dictionary<Side, bool> wallMask = new Dictionary<Side, bool> (); // Dictionary walls - blocks the movement of chips in certain directions
	
    public SlotGravity slotGravity;
    public SlotTeleport slotTeleport;

    public bool sugarDropSlot = false;
    public static Transform folder;
	
	void  Awake (){
		slotGravity = GetComponent<SlotGravity>();
        slotTeleport = GetComponent<SlotTeleport>();
	}

	public static void  Initialize (){
        foreach (Slot slot in FindObjectsOfType<Slot>())
            if (!all.ContainsKey(slot.coord))
                all.Add(slot.coord, slot);


        foreach (Slot slot in all.Values) {
            foreach (Side side in Utils.allSides) // Filling of the nearby slots dictionary 
                slot.nearSlot.Add(side, all.ContainsKey(slot.coord + side) ? all[slot.coord + side] : null);
            slot.nearSlot.Add(Side.Null, null);
		    foreach (Side side in Utils.straightSides) // Filling of the walls dictionary
                slot.wallMask.Add(side, false);
        }

        Side direction;
        SlotTeleport teleport;
        foreach (Slot slot in all.Values) {
            direction = slot.slotGravity.gravityDirection;
            if (slot[direction]) {
                slot[direction].slotGravity.fallingDirection = Utils.MirrorSide(direction);
            }
            teleport = slot.GetComponent<SlotTeleport>();
            if (teleport)
                teleport.Initialize();
        }
	}

    Chip _chip;
    public Chip chip {
        get {
            return _chip;
        }
        set {
            if (value == null) {
                if (_chip)
                    _chip.slot = null;
                _chip = null;
                return;
            }
            if (_chip)
                _chip.slot = null;
            _chip = value;
            _chip.transform.parent = transform;
            if (_chip.slot)
                _chip.slot.chip = null;
            _chip.slot = this;
        }
    }

    public void SetScore(float s) {
        SessionAssistant.main.score += Mathf.RoundToInt(s * SessionAssistant.scoreC);
        ScoreBubble.Bubbling(Mathf.RoundToInt(s * SessionAssistant.scoreC), transform);
    }

	// Check for the presence of the "shadow" in the slot. No shadow - is a direct path from the slot up to the slot with a component SlotGenerator. Towards must have slots (without blocks and wall)
	// This concept is very important for the proper physics chips
	public bool GetShadow (){
		if (slotGravity) return slotGravity.shadow;
		else return false;
	}

	// Shadow can also discard the other chips - it's a different kind of shadow.
	public bool GetChipShadow (){
        Side direction = slotGravity.fallingDirection;
        Slot s = nearSlot[direction];
		for (int i = 0; i < 40; i ++) {
			if (!s) return false;
			if (s.block)  return false;
			if (!s.chip || s.slotGravity.gravityDirection != direction) {
                direction = s.slotGravity.fallingDirection;
                s = s.nearSlot[direction];
            } else return true;
		}
        return false;
	}
	
	// creating a wall between it and neighboring slot
	public void  SetWall (Side side){

		wallMask[side] = true;

		foreach (Side s in Utils.straightSides)
			if (wallMask[s]) {
				if (nearSlot[s]) nearSlot[s].nearSlot[Utils.MirrorSide(s)] = null;
				nearSlot[s] = null;
			}
	
		foreach (Side s in Utils.slantedSides)
			if (wallMask[Utils.SideHorizontal(s)] && wallMask[Utils.SideVertical(s)]) {
				if (nearSlot[s]) nearSlot[s].nearSlot[Utils.MirrorSide(s)] = null;
				nearSlot[s] = null;
			}

	}

    public static Slot GetSlot(int2 position) {
        if (all.ContainsKey(position))
            return all[position];
        return null;
    }

    public static Slot GetSlot(int x, int y, int z) {
        return GetSlot(new int2(x, y));
    }
}