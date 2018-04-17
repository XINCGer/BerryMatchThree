using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

// The class is responsible for logic CrossBomb
[RequireComponent (typeof (Chip))]
public class CrossBomb : IBomb, IAnimateChip, IChipLogic {

    public string type = "CrossBomb";
    public Side[] sides = new Side[4] {
        Side.Left,
        Side.Right,
        Side.Top,
        Side.Bottom
    };

	Chip _chip;
	int birth; // Event count at the time of birth SessionAssistant.main.eventCount
    int branchCount;

    public Chip chip {
        get {
            return _chip;
        }
    }
	
	void  Awake (){
		_chip = GetComponent<Chip>();
		birth = SessionAssistant.main.eventCount;
		AudioAssistant.Shot ("CreateCrossBomb");
	}

    // Coroutine destruction / activation
    public IEnumerator Destroying() {
        if (birth == SessionAssistant.main.eventCount) {
            chip.destroying = false;
            yield break;
        }

        chip.busy = true;

        while (transform.localPosition != Vector3.zero) {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Time.deltaTime * 10);
            yield return 0;
        }

        List<Side> _sides = new List<Side>(sides);
        if (!_sides.Contains(Side.Left))
            Destroy(transform.Find("SideLeft").gameObject);
        if (!_sides.Contains(Side.Right))
            Destroy(transform.Find("SideRight").gameObject);
        if (!_sides.Contains(Side.Top))
            Destroy(transform.Find("SideTop").gameObject);
        if (!_sides.Contains(Side.Bottom))
            Destroy(transform.Find("SideBottom").gameObject);

        chip.Play("Destroying");
        AudioAssistant.Shot("CrossBombCrush");

        int2 coord = chip.slot.coord;
        chip.ParentRemove();

        FieldAssistant.main.BlockCrush(coord, false);
        FieldAssistant.main.JellyCrush(coord);


        int count = 4;
        for (int path = 1; count > 0; path++) {
            count = 0;
            foreach (Side side in sides) {
                if (Freez(coord + Utils.SideOffset(side) * path))
                    count++;
            }
        }

        count = 4;
        for (int path = 1; count > 0; path++) {
            count = 0;
            yield return new WaitForSeconds(0.02f);
            foreach (Side side in sides)
                if (Crush(coord + Utils.SideOffset(side) * path, chip.jamType))
                    count++;
        }

        yield return new WaitForSeconds(0.1f);

        chip.busy = false;

        while (chip.IsPlaying("Destroying"))
            yield return 0;
        Destroy(gameObject);
    }

    public static bool Crush(int2 coord, string jamType = "") {
        Slot s = Slot.GetSlot(coord);
        FieldAssistant.main.BlockCrush(coord, false, true);
        FieldAssistant.main.JellyCrush(coord);
        if (s)
            Jam.JamIt(s, jamType);
        if (s && s.chip) {
            Chip c = s.chip;
            c.SetScore(0.3f);
            c.jamType = jamType;
            c.DestroyChip();
            AnimationAssistant.main.Explode(s.transform.position, 3, 20);
        }
        return coord.IsItHit(0, 0, LevelProfile.main.width - 1, LevelProfile.main.height - 1);
    }

    public static bool Freez(int2 coord) {
        Slot s = Slot.GetSlot(coord);
        if (s && s.chip && !s.chip.busy && s.chip.destroyable && !(s.block && s.block is Branch)) {
            s.chip.busy = true;
        }

        return coord.IsItHit(0, 0, LevelProfile.main.width - 1, LevelProfile.main.height - 1);
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip) || !chip.slot)
            return stack;

        stack.Add(chip);

        Slot s;
        for (int path = 1; path < LevelProfile.maxSize; path++) {
            foreach (Side side in sides) {
                s = Slot.GetSlot(chip.slot.coord + Utils.SideOffset(side) * path);
                if (s && s.chip)
                    stack = s.chip.GetDangeredChips(stack);
            }
        }

        return stack;
    }

    #region Mixes

    public void CrossSimpleMix(Chip secondary) {
        StartCoroutine(CrossSimpleMixRoutine(secondary));
    }

    IEnumerator CrossSimpleMixRoutine(Chip secondary) { 
        chip.busy = true;
        chip.destroyable = false;
        SessionAssistant.main.EventCounter();

        chip.jamType = chip.slot.jam ? chip.slot.jam.type : "";

        Transform effect = ContentAssistant.main.GetItem("SimpleCrossMixEffect").transform;
        effect.SetParent(Slot.folder);
        effect.position = transform.position;
        effect.GetComponent<Animation>().Play();
        AudioAssistant.Shot("CrossBombCrush");

        List<Side> _sides = new List<Side>(sides);
        if (!_sides.Contains(Side.Left)) 
            Destroy(effect.Find("WaveLeft").gameObject);
        if (!_sides.Contains(Side.Right))
            Destroy(effect.Find("WaveRight").gameObject);
        if (!_sides.Contains(Side.Top))
            Destroy(effect.Find("WaveTop").gameObject);
        if (!_sides.Contains(Side.Bottom))
            Destroy(effect.Find("WaveBottom").gameObject);

        chip.Minimize();

        yield return new WaitForSeconds(0.1f);

        FieldAssistant.main.JellyCrush(chip.slot.coord);
        FieldAssistant.main.BlockCrush(chip.slot.coord, false);

        System.Action<int2> Wave = (int2 coord) => {
            Slot s = Slot.GetSlot(coord);
            if (s)
                AnimationAssistant.main.Explode(s.transform.position, 3, 20);
        };

        for (int path = 0; path < LevelProfile.maxSize; path++) {
            foreach (Side side in _sides) {
                Freez(chip.slot.coord + Utils.SideOffset(side) * path);
                Freez(chip.slot.coord + Utils.SideOffset(side) * path + Utils.SideOffset(Utils.RotateSide(side, 2)));
                Freez(chip.slot.coord + Utils.SideOffset(side) * path + Utils.SideOffset(Utils.RotateSide(side, -2)));
            }
        }

        foreach (Side side in Utils.allSides)
            Crush(chip.slot.coord + Utils.SideOffset(side), chip.jamType);
        Wave(chip.slot.coord);

        yield return new WaitForSeconds(0.05f);

        for (int path = 2; path < LevelProfile.maxSize; path++) {
            foreach (Side side in _sides) {
                Crush(chip.slot.coord + Utils.SideOffset(side) * path, chip.jamType);
                Crush(chip.slot.coord + Utils.SideOffset(side) * path + Utils.SideOffset(Utils.RotateSide(side, 2)), chip.jamType);
                Crush(chip.slot.coord + Utils.SideOffset(side) * path + Utils.SideOffset(Utils.RotateSide(side, -2)), chip.jamType);
                Wave(chip.slot.coord + Utils.SideOffset(side) * path);
            }
            yield return new WaitForSeconds(0.05f);
        }

        chip.busy = false;
        chip.HideChip(false);
    }

    public void CrossMix(Chip secondary) {
        StartCoroutine(CrossMixRoutine(secondary));
    }

    IEnumerator CrossMixRoutine(Chip secondary) {
        chip.busy = true;
        chip.destroyable = false;
        SessionAssistant.main.EventCounter();

        Transform effect = ContentAssistant.main.GetItem("CrossMixEffect").transform;
        effect.SetParent(Slot.folder);
        effect.position = transform.position;
        effect.GetComponent<Animation>().Play();
        AudioAssistant.Shot("CrossBombCrush");

        chip.Minimize();

        FieldAssistant.main.BlockCrush(chip.slot.coord, false);
        FieldAssistant.main.JellyCrush(chip.slot.coord);

        int count = 8;
        for (int path = 1; count > 0; path++) {
            count = 0;
            foreach (Side side in Utils.allSides)
                if (Freez(chip.slot.coord + Utils.SideOffset(side) * path))
                    count++;
        }

        if (chip.slot.jam)
            chip.jamType = chip.slot.jam.type;

        count = 8;
        for (int path = 1; count > 0; path++) {
            count = 0;
            yield return new WaitForSeconds(0.05f);
            foreach (Side side in Utils.allSides)
                if (Crush(chip.slot.coord + Utils.SideOffset(side) * path, chip.jamType)) 
                    count++;
        }

        chip.ParentRemove();

        yield return new WaitForSeconds(0.1f);

        chip.busy = false;
        chip.HideChip(false);
    }

    public void LineMix(Chip secondary) {
        sides = Utils.straightSides;
        chip.DestroyChip();
    }

    public string[] GetClipNames() {
        return new string[] { "Destroying" };
    }

    public string GetChipType() {
        return type;
    }

    public bool IsMatchable() {
        return true;
    }

    public int GetPotencial() {
        return LevelProfile.main.height + LevelProfile.main.width - 1;
    }
    #endregion

}