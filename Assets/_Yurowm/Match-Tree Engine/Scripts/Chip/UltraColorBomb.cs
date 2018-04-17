using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The class is responsible for logic ColorBomb
[RequireComponent(typeof(Chip))]
public class UltraColorBomb : IBomb, IAnimateChip, IChipLogic {

    Chip _chip;
    int birth; // Event count at the time of birth SessionAssistant.main.eventCount

    public Chip chip {
        get {
            return _chip;
        }
    }

    void Awake() {
        _chip = GetComponent<Chip>();
        birth = SessionAssistant.main.eventCount;
        AudioAssistant.Shot("CreateColorBomb");
    }

    // Coroutine destruction / activation
    public IEnumerator Destroying() {
        if (birth == SessionAssistant.main.eventCount) {
            chip.destroying = false;
            yield break;
        }


        SimpleChip[] chips = FindObjectsOfType<SimpleChip>();
        if (chips.Length == 0)
            yield break;
        chip.id = chips[Random.Range(0, chips.Length)].chip.id;
        chip.chipType = "SimpleChip";
        yield return StartCoroutine(UltraColorMixRoutine(chip));
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip))
            return stack;

        stack.Add(chip);

        int color_id = Random.Range(0, LevelProfile.main.colorCount);
        foreach (Slot slot in Slot.all.Values) {
            if (slot.coord == chip.slot.coord) continue;
            if (slot.chip && slot.chip.id == color_id && !stack.Contains(slot.chip))
                stack.AddRange(slot.chip.GetDangeredChips(stack));
        }
        return stack;
    }

    #region Mixes
    void UltraColorMix(Chip secondary) {
        StartCoroutine(UltraColorMixRoutine(secondary));
    }

    IEnumerator UltraColorMixRoutine(Chip secondary) {
        chip.busy = true;
        chip.destroyable = false;

        chip.Play("Hit");
        AudioAssistant.Shot("ColorBombCrush");

        FieldAssistant.main.JellyCrush(chip.slot.coord);
        
        if (chip.slot.jam)
            chip.jamType = chip.slot.jam.type;
        List<Chip> target = new List<Chip>();
        foreach (Slot slot in Slot.all.Values) {
                if (slot == chip.slot)
                    continue;
                if (slot.chip == null || slot.chip == secondary)
                    continue;
                if (secondary.chipType == "UltraColorBomb" || slot.chip.id == secondary.id) {
                    yield return new WaitForSeconds(0.02f);
                    if (slot.chip) {
                        if (secondary.chipType != "SimpleChip" && secondary.chipType != "UltraColorBomb")
                            FieldAssistant.main.AddPowerup(slot.coord, secondary.chipType);
                        Lightning.CreateLightning(3, transform, slot.chip.transform, slot.chip.IsColored() ? Chip.colors[slot.chip.id] : Color.white);
                        target.Add(slot.chip);
                    }
                }
            
        }

        yield return new WaitForSeconds(0.1f);

        SessionAssistant.main.EventCounter();
        foreach(Chip t in target) {
            if (t.destroying)
                continue;
            t.SetScore(0.3f);
            FieldAssistant.main.BlockCrush(t.slot.coord, true);
            FieldAssistant.main.JellyCrush(t.slot.coord);
            t.jamType = chip.jamType;
            t.DestroyChip();
            yield return new WaitForSeconds(0.02f);
        }

        yield return new WaitForSeconds(0.1f);

        FieldAssistant.main.JellyCrush(chip.slot.coord);

        while (chip.IsPlaying("Hit"))
            yield return 0;

        chip.Play("Destroying");

        while (chip.IsPlaying("Destroying"))
            yield return 0;

        chip.busy = false;
        chip.ParentRemove();

        Destroy(gameObject);
    }

    public string[] GetClipNames() {
        return new string[] { "Destroying" };
    }

    public string GetChipType() {
        return "UltraColorBomb";
    }

    public bool IsMatchable() {
        return false;
    }

    public int GetPotencial() {
        return Slot.all.Count / LevelProfile.main.colorCount;
    }
    #endregion
}