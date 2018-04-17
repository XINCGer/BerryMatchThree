using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;
using System.Linq;

// Generator of playing field
public class FieldAssistant : MonoBehaviour {

    public static FieldAssistant main;

	[HideInInspector]
	public Field field;
	
	void  Awake (){
		main = this;
	}

    public void StartLevel() {
        StartCoroutine(StartLevelRoutine());
    }

    IEnumerator StartLevelRoutine() {
        UIAssistant.main.ShowPage("Loading");

        while (CPanel.uiAnimation > 0)
            yield return 0;

        ProfileAssistant.main.local_profile["life"]--;

        SessionAssistant.main.enabled = false;

		SessionAssistant.Reset();
        
        yield return StartCoroutine(CreateField());

		SessionAssistant.main.enabled = true;
		SessionAssistant.main.eventCount ++;

        SessionAssistant.main.StartSession(LevelProfile.main.target, LevelProfile.main.limitation);

        GameCamera.main.transform.position = new Vector3(0, 20, -10);
    }

	// Field generator
	public IEnumerator  CreateField (){
		RemoveField (); // Removing old field
		
		field = new Field (LevelProfile.main.GetClone());

        Slot.folder = new GameObject().transform;
        Slot.folder.name = "Slots";
        

        Slot.all.Clear();

        Vector3 fieldDimensions = new Vector3(field.width - 1, field.height - 1, 0) * ProjectParameters.main.slot_offset;

        foreach (SlotSettings settings in field.slots.Values) {
            yield return 0;

            Slot slot;

            #region Creating a new empty slot
            Vector3 position = new Vector3(settings.position.x, settings.position.y, 0) * ProjectParameters.main.slot_offset - fieldDimensions / 2;
            GameObject obj = ContentAssistant.main.GetItem("SlotEmpty", position);
            obj.name = "Slot_" + settings.position.x + "x" + settings.position.y;
            obj.transform.SetParent(Slot.folder);
            slot = obj.GetComponent<Slot>();
            slot.coord = settings.position;
            Slot.all.Add(slot.coord, slot);
            #endregion

            #region Creating a generator
            if (settings.generator)
                slot.gameObject.AddComponent<SlotGenerator>();
            #endregion

            #region Creating a teleport
            if (settings.teleport != int2.Null)
                slot.slotTeleport.target_postion = settings.teleport;
            else
                Destroy(slot.slotTeleport);
            #endregion

            #region Setting gravity direction
            slot.slotGravity.gravityDirection = settings.gravity;
            #endregion

            #region Setting sugar target (by slot tag)
            if (LevelProfile.main.target == FieldTarget.SugarDrop && settings.tags.Contains("SugarDrop")) {
                slot.sugarDropSlot = true;
                GameObject sd = ContentAssistant.main.GetItem("SugarDrop", position);
                sd.name = "SugarDrop";
                sd.transform.parent = slot.transform;
                sd.transform.localPosition = Vector3.zero;
                sd.transform.Rotate(0, 0, Utils.SideToAngle(settings.gravity) + 90);
            }
            #endregion

            #region Creating a block
            if (settings.block_type != "") {
                GameObject b_obj = ContentAssistant.main.GetItem(settings.block_type);
                b_obj.transform.SetParent(slot.transform);
                b_obj.transform.localPosition = Vector3.zero;
                b_obj.name = settings.block_type + "_" + settings.position.x + "x" + settings.position.y;
                IBlock block = b_obj.GetComponent<IBlock>();
                slot.block = block;
                block.slot = slot;
                block.level = settings.block_level;
                block.Initialize();
            }
            #endregion

            #region Create a jelly
            if (LevelProfile.main.target == FieldTarget.Jelly && settings.jelly_level > 0) {
                GameObject j_obj;
                switch (settings.jelly_level) {
                    case 1: j_obj = ContentAssistant.main.GetItem("SingleLayerJelly");
                        break;
                    case 2:
                    default: j_obj = ContentAssistant.main.GetItem("Jelly");
                        break;
                }
                j_obj.transform.SetParent(slot.transform);
                j_obj.transform.localPosition = Vector3.zero;
                j_obj.name = "Jelly_" + settings.position.x + "x" + settings.position.y;
                Jelly jelly = j_obj.GetComponent<Jelly>();
                slot.jelly = jelly;
            }
            #endregion

            #region Create a jam
            if (LevelProfile.main.target == FieldTarget.Jam) {
                Jam.JamIt(slot, settings.jam);
            }
            #endregion

            #region Create a chip
            if (!string.IsNullOrEmpty(settings.chip) && (slot.block == null || slot.block.CanItContainChip())) {
                SessionAssistant.ChipInfo chipInfo = SessionAssistant.main.chipInfos.Find(x => x.name == settings.chip);
                if (chipInfo != null) {
                    string key = chipInfo.contentName + (chipInfo.color ? Chip.chipTypes[Mathf.Clamp(settings.color_id, 0, Chip.colors.Length - 1)] : "");
                    GameObject c_obj = ContentAssistant.main.GetItem(key);
                    c_obj.transform.SetParent(slot.transform);
                    c_obj.transform.localPosition = Vector3.zero;
                    c_obj.name = key;
                    slot.chip = c_obj.GetComponent<Chip>();
                }
            }
            #endregion
        }

        Slot.Initialize();

        foreach (int2 coord in field.wall_vertical) {
            yield return 0;
            #region Create a vertical wall 
            Slot slot = Slot.GetSlot(coord);
            if (slot) {
                slot.SetWall(Side.Left);
                slot = slot[Side.Left];
                if (slot)
                    slot.SetWall(Side.Right);
            }
            #endregion
        }

        foreach (int2 coord in field.wall_horizontal) {
            yield return 0;
            #region Create a horizontal wall
            Slot slot = Slot.GetSlot(coord);
            if (slot) {
                slot.SetWall(Side.Bottom);
                slot = slot[Side.Bottom];
                if (slot)
                    slot.SetWall(Side.Top);
            }
            #endregion
        }

        List<Pair<int2>> walls = new List<Pair<int2>>();
        foreach (Slot slot in Slot.all.Values) {
            foreach (Side side in Utils.straightSides) {
                if (slot[side] != null) continue;

                Pair<int2> pair = new Pair<int2>(slot.coord, slot.coord + Utils.SideOffset(side));
                if (walls.Contains(pair))
                    continue;

                yield return 0;
                #region Create a wall object
                Vector3 position = new Vector3(Utils.SideOffsetX(side), Utils.SideOffsetY(side), 0) * 0.353f;

                GameObject w_obj = ContentAssistant.main.GetItem("Wall");
                w_obj.transform.SetParent(slot.transform);
                w_obj.transform.localPosition = position;
                w_obj.name = "Wall_" + side;
                if (Utils.SideOffsetY(side) != 0)
                    w_obj.transform.Rotate(0, 0, 90);

                walls.Add(pair);
                #endregion
            }
        }

        SlotGravity.Reshading();

        yield return 0;

        SUBoosterButton.Generate(Slot.folder);
	}

	// Removing old field
	public void  RemoveField (){
        if (Slot.folder)
            Destroy(Slot.folder.gameObject);
	}

	// Creating a simple random color chips
	public Chip GetNewSimpleChip (int2 coord, Vector3 position){
		return GetNewSimpleChip(coord, position, SessionAssistant.main.colorMask[Random.Range(0, field.colorCount)]);
	}

    // Creating a sugar chips
    public Chip GetSugarChip(int2 coord, Vector3 position) {
        GameObject o = ContentAssistant.main.GetItem("Sugar");
        o.transform.position = position;
        o.name = "Sugar";
        if (Slot.GetSlot(coord).chip)
            o.transform.position = Slot.GetSlot(coord).chip.transform.position;
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(coord).chip = chip;
        return chip;
    }

    public Chip GetNewStone (int2 coord, Vector3 position) {
		GameObject o = ContentAssistant.main.GetItem ("Stone");
		o.transform.position = position;
        o.name = "Stone";
        if (Slot.GetSlot(coord).chip)
            o.transform.position = Slot.GetSlot(coord).chip.transform.position;
		Chip chip = o.GetComponent<Chip> ();
        Slot.GetSlot(coord).chip = chip;
		return chip;
	}

	// Creating a simple chip specified color
	public Chip GetNewSimpleChip (int2 coord, Vector3 position, int id) {
        GameObject o = ContentAssistant.main.GetItem("SimpleChip" + Chip.chipTypes[id]);
		o.transform.position = position;
        o.name = "Chip_" + Chip.chipTypes[id];
        if (Slot.GetSlot(coord).chip)
            o.transform.position = Slot.GetSlot(coord).chip.transform.position;
		Chip chip = o.GetComponent<Chip> ();
		Slot.GetSlot(coord).chip = chip;
		return chip;
	}

	// Creating a cross-bombs specified color
	public Chip GetNewCrossBomb (int2 coord, Vector3 position, int id){
        GameObject o = ContentAssistant.main.GetItem("CrossBomb" + Chip.chipTypes[id]);
		o.transform.position = position;
        o.name = "CrossBomb_" + Chip.chipTypes[id];
        if (Slot.GetSlot(coord).chip)
            o.transform.position = Slot.GetSlot(coord).chip.transform.position;
		Chip chip = o.GetComponent<Chip> ();
        Slot.GetSlot(coord).chip = chip;
		return chip;
	}

    public Chip GetNewBomb(int2 coord, string powerup, Vector3 position, int id) {
        SessionAssistant.ChipInfo p = SessionAssistant.main.chipInfos.Find(pu => pu.name == powerup);
        if (p == null)
            return null;
        id = Mathf.Clamp(id, 0, Chip.colors.Length);
        GameObject o = ContentAssistant.main.GetItem(p.contentName + (p.color ? Chip.chipTypes[id] : ""));
        o.transform.position = position;
        o.name = p.contentName + (p.color ? Chip.chipTypes[id] : "");
        if (Slot.GetSlot(coord).chip)
            o.transform.position = Slot.GetSlot(coord).chip.transform.position;
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(coord).chip = chip;
        return chip;
    }

	// Make a bomb in the specified location with the ability to transform simple chips in a bomb
	public Chip AddPowerup(int2 coord, string powerup) {
        Slot slot = Slot.GetSlot(coord).GetComponent<Slot>();
		Chip chip = slot.chip;
		int id;
		if (chip)
			id = chip.id;
		else 
			id = Random.Range(0, Chip.colors.Length);
		if (chip)
			Destroy (chip.gameObject);

        chip = GetNewBomb(slot.coord, powerup, slot.transform.position, id);
		return chip;
	}

	// Create a bomb with the possibility of transformation of simple chips in bomb
    public void AddPowerup(string powerup) {
		List<SimpleChip> chips = new List<SimpleChip>(FindObjectsOfType<SimpleChip>());
		if (chips.Count == 0) return;
		SimpleChip chip = chips.Where(x => x != null && !x.chip.busy).ToList().GetRandom();
		Slot slot = chip.chip.slot;
		if (slot)
            AddPowerup(slot.coord, powerup);
	}

	// Crush jelly function
	public void JellyCrush (int2 coord) {
        Slot s = Slot.GetSlot(coord);
        if (s && s.jelly)
            s.jelly.JellyCrush();
	}

	// Crush block function
   	public void  BlockCrush (int2 coord, bool radius, bool force = false) {
		IBlock block = null;
		Slot slot = null;
		Chip chip = null;
		StoneChip stone = null;


		if (radius) {
			foreach (Side side in Utils.straightSides) {
				block = null;
				slot = null;
				chip = null;
				stone = null;

				slot = Slot.GetSlot(coord + side);
                if (!slot)
                    continue;

                block = slot.block;
                if (block && block.CanBeCrushedByNearSlot())
                    block.BlockCrush(force);
                				
				if (slot) chip = slot.chip;
				if (chip) stone = chip.GetComponent<StoneChip>();
				if (stone) chip.DestroyChip();
			}
		}

        slot = Slot.GetSlot(coord);
        if (slot)
            block = slot.block;
		if (block)
            block.BlockCrush(force);
	}
}

// The class information about the playing field and the target level
public class Field {
	public int width;
	public int height;
	public int colorCount;
    public Dictionary<int2, SlotSettings> slots = new Dictionary<int2, SlotSettings>();
    public List<int2> wall_horizontal = new List<int2>();
    public List<int2> wall_vertical = new List<int2>();
    public int targetValue = 0;
		
	public Field (LevelProfile profile){
		width = profile.width;
		height = profile.height;
        colorCount = profile.colorCount;
        foreach (SlotSettings slot in profile.slots)
            if (!slots.ContainsKey(slot.position))
                slots.Add(slot.position, slot.GetClone());
        wall_horizontal = new List<int2>(profile.wall_horizontal);
        wall_vertical = new List<int2>(profile.wall_vertical);
        FirstChipGeneration();
    }

    public SlotSettings GetSlot(int2 pos) {
        if (slots.ContainsKey(pos))
            return slots[pos];
        return null;
    }

	public SlotSettings GetSlot (int x, int y){
		return GetSlot(new int2(x, y));
	}

    int NewRandomChip(int2 coord) {
        List<int> ids = new List<int>();
        for (int i = 0; i < colorCount; i++)
            ids.Add(i + 1);

        foreach (Side side in Utils.straightSides)
            if (slots.ContainsKey(coord + side) && ids.Contains(slots[coord + side].color_id))
                ids.Remove(slots[coord + side].color_id);

        if (ids.Count > 0)
            return ids.GetRandom();
        else
            return Random.Range(0, colorCount);
    }

    public void  FirstChipGeneration (){

        // replace random chips on nonrandom
        foreach (int2 pos in slots.Keys)
            if (slots[pos].color_id == 0)
                slots[pos].color_id = NewRandomChip(pos);

        foreach (int2 pos in slots.Keys)
            if (slots[pos].color_id > 0)
                slots[pos].color_id--;



        int[] a = new int[Chip.colors.Length];
        // a => 0, 1, 2, 3, 4...
        for (int i = 0; i < a.Length; i++)
            a[i] = i;

		for (int i = Chip.colors.Length - 1; i > 0; i--) {
            int j = Random.Range(0, i);
			a[j] = a[j] + a[i];
			a[i] = a[j] - a[i];
			a[j] = a[j] - a[i];
		}

		SessionAssistant.main.colorMask = a;

        // apply the results to the matrix shuffling chips	
        foreach (int2 pos in slots.Keys)
            if (slots[pos].color_id >= 0 && slots[pos].color_id < a.Length)
                slots[pos].color_id = a[slots[pos].color_id];
	}
	
}

public enum FieldTarget {
	None = 0,
	Jelly = 1,
	Block = 2,
    Color = 3,
    SugarDrop = 4,
    Jam = 5,
}

public enum Limitation {
	Moves,
	Time
}