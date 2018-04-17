using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

// The class is responsible for logic ColorBomb
[RequireComponent (typeof (Chip))]
public class ColorBomb : IBomb, IAnimateChip, IChipLogic {

	int birth; // Event count at the time of birth SessionAssistant.main.eventCount
	public Color color;

	Chip _chip;
    public Chip chip {
        get {
            return _chip;
        }
    }
	
	void  Awake (){
		_chip = GetComponent<Chip>();
		birth = SessionAssistant.main.eventCount;
		AudioAssistant.Shot ("CreateColorBomb");
	}

    // Coroutine destruction / activation
    public IEnumerator Destroying() {
        if (birth == SessionAssistant.main.eventCount) {
			chip.destroying = false;
			yield break;
		}
        
        chip.busy = true;

        chip.Play("Destroying");
        AudioAssistant.Shot("ColorBombCrush");
		
		Slot s;

        if (chip.slot)
            FieldAssistant.main.JellyCrush(chip.slot.coord);

        chip.gravity = false;

        int2 key = new int2();
		for (key.x = 0; key.x < LevelProfile.main.width; key.x++) {
			for (key.y = 0; key.y < LevelProfile.main.height; key.y++) {
				if (key == chip.slot.coord) continue;
                s = Slot.GetSlot(key);
				if (s && s.chip && s.chip.id == chip.id) {
					Lightning.CreateLightning(3, transform, s.chip.transform, color);
                    yield return new WaitForSeconds(0.03f);
				}
			}
		}
		
		yield return new WaitForSeconds(0.1f);
		
		for (key.x = 0; key.x < LevelProfile.main.width; key.x++) {
			for (key.y = 0; key.y < LevelProfile.main.height; key.y++) {
				if (key == chip.slot.coord) continue;
                s = Slot.GetSlot(key);
				if (s && s.chip && s.chip.id == chip.id) {
					s.chip.SetScore(0.3f);
					FieldAssistant.main.BlockCrush(key, true);
					FieldAssistant.main.JellyCrush(key);
                    s.chip.jamType = chip.jamType;
                    s.chip.DestroyChip();
                    yield return new WaitForSeconds(0.02f);
				}
			}
		}
		
		yield return new WaitForSeconds(0.1f);
		chip.busy = false;
        chip.gravity = true;

        while (chip.IsPlaying("Destroying")) yield return 0;
		chip.ParentRemove();
        Destroy(gameObject);
	}

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip))
            return stack;

        stack.Add(chip);

        Slot s;

        int2 key = new int2();
		for (key.x = 0; key.x < LevelProfile.main.width; key.x++) {
			for (key.y = 0; key.y < LevelProfile.main.height; key.y++) {
				if (key == chip.slot.coord) continue;
                s = Slot.GetSlot(key);
                if (s && s.chip && s.chip.id == chip.id)
                    stack = s.chip.GetDangeredChips(stack);
            }
        }
        return stack;
    }

    #region Mixes
    public void ColorMix(Chip secondary) {
        StartCoroutine(ColorMixRoutine(secondary));
    }

    IEnumerator ColorMixRoutine(Chip secondary) {
        chip.busy = true;
        chip.gravity = false;
        chip.destroyable = false;

        SimpleChip[] allChips = FindObjectsOfType<SimpleChip>();
        List<SimpleChip>[] sorted = new List<SimpleChip>[Chip.colors.Length];
        int[] count = new int[Chip.colors.Length];

        foreach (SimpleChip c in allChips) {
            if (c.chip.destroying)
                continue;
            if (!c.chip.slot)
                continue;
            if (c.chip == secondary)
                continue;
            count[c.chip.id]++;
            if (sorted[c.chip.id] == null)
                sorted[c.chip.id] = new List<SimpleChip>();
            sorted[c.chip.id].Add(c);
        }

        List<Slot> targets = new List<Slot>();

        int i;
        for (i = 0; i < Chip.colors.Length; i++)
            if (sorted[i] != null && sorted[i].Count > 0)
                targets.Add(sorted[i].GetRandom().chip.slot);

        yield return new WaitForSeconds(0.1f);

        AudioAssistant.Shot("ColorBombCrush");
        chip.Play("Destroying");

        foreach (Slot target in targets) {
            Chip pu = FieldAssistant.main.AddPowerup(target.coord, secondary.chipType);
            target.chip = pu;
            Lightning.CreateLightning(3, transform, pu.transform, Chip.colors[chip.id]);
            yield return new WaitForSeconds(0.1f);

        }

        yield return new WaitForSeconds(0.2f);

        if (chip.slot.jam)
            chip.jamType = chip.slot.jam.type;
        SessionAssistant.main.EventCounter();
        if (secondary.chipType != "UltraColorBomb")
            for (i = 0; i < targets.Count; i++) {
                if (targets[i].chip) {
                    targets[i].chip.jamType = chip.jamType;
                    targets[i].chip.DestroyChip();
                }
                yield return new WaitForSeconds(0.05f);
            }

        chip.busy = false;
        chip.gravity = true;

        FieldAssistant.main.JellyCrush(chip.slot.coord);

        while (chip.IsPlaying("Destroying"))
            yield return 0;

        chip.ParentRemove();
        chip.HideChip(false);
    }

    public string[] GetClipNames() {
        return new string[] { "Destroying" };
    }

    public string GetChipType() {
        return "ColorBomb";
    }

    public bool IsMatchable() {
        return true;
    }

    public int GetPotencial() {
        return Mathf.RoundToInt(1f * Slot.all.Count / LevelProfile.main.colorCount);
    }
    #endregion
}