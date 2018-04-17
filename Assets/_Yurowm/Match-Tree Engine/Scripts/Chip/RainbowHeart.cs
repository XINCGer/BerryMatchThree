using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Berry.Utils;

[RequireComponent (typeof (Chip))]
public class RainbowHeart : IBomb, IAnimateChip, IChipLogic {

    int birth; // Event count at the time of birth SessionAssistant.main.eventCount
	public Chip _chip;

    List<Chip> chips = new List<Chip>();
    int branchCount;

    public Chip chip {
        get {
            return _chip;
        }
    }

    void Awake() {
        birth = SessionAssistant.main.eventCount;
		_chip = GetComponent<Chip>();
        AudioAssistant.Shot("RainbowHeartCreate");
	}


    // Coroutine destruction / activation

    public IEnumerator Destroying() {
        yield return StartCoroutine(DestroyChipFunction(""));
    }

    IEnumerator  DestroyChipFunction (string powerup) {
        if (birth == SessionAssistant.main.eventCount) {
            chip.destroying = false;
            yield break;
        }

        if (!chip.destroying)
            yield break;

        chip.busy = true;
        chip.gravity = false;

        chip.Play("Match");
        AudioAssistant.Shot("RainbowHeartCrush");
        SessionAssistant.main.EventCounter();

        FieldAssistant.main.JellyCrush(chip.slot.coord);

        chips.Add(chip);
        branchCount = GetBranchCount();

        if (chip.slot.jam)
            chip.jamType = chip.slot.jam.type;
        for (int i = 0; i < branchCount; i++)
            StartCoroutine(LightningBranch(powerup));

        while (branchCount != -1)
            yield return 0;

        chip.ParentRemove();
        chip.busy = false;
        chip.gravity = true;
    
        chip.Play("Destroying");
        while (chip.IsPlaying("Destroying"))
            yield return 0;

        Destroy(gameObject);
	}

    IEnumerator LightningBranch(string powerup = "") {
        yield return new WaitForSeconds(0.1f);
       
        Slot currentSlot = chip.slot;
        Chip nextChip;
        Slot nextSlot;
        Lightning lightning = null;
        int iter = 10;
        int count = 10;

        List<Chip> branch = new List<Chip>();

        while (true) {
            if (iter <= 0 || count <= 0)
                break;

            nextSlot = currentSlot.nearSlot.Values.Where(x => x != null).ToList().GetRandom();
            if (!nextSlot) {
                iter--;
                continue;
            }
            nextChip = nextSlot.chip;
            if (!nextChip || nextChip.destroying) {
                iter--;
                continue;
            }
            if (!currentSlot.chip) {
                iter--;
                continue;
            }

            if (chips.Contains(nextChip) || branch.Contains(nextChip)) {
                iter--;
                continue;
            }

            chips.Add(nextChip);
            branch.Add(nextChip);

            int id = nextChip.id;

            if (lightning != null) {
                lightning.Remove();
            }
            lightning = Lightning.CreateLightning(0, currentSlot.chip.transform, nextChip.transform, id == Mathf.Clamp(id, 0, 5) ? Chip.colors[id] : Color.white);

            count--;

            currentSlot.chip.SetScore(0.3f);
            currentSlot.chip.jamType = chip.jamType;
            currentSlot.chip.DestroyChip();
            currentSlot = nextSlot;

            yield return new WaitForSeconds(0.02f);
        }


        if (powerup != "") {
            bool dontDestroy = powerup == "RainbowHeart" || powerup == "UltraColorBomb";
            Chip pu = FieldAssistant.main.AddPowerup(currentSlot.coord, powerup);
            //if (!dontDestroy)
            //    pu.can_move = false;
            pu.transform.localPosition = Vector3.zero;

            yield return 0;
            while (!dontDestroy && pu != null && pu.GetComponent<Animation>().isPlaying)
                yield return 0;
            if (pu != null && !dontDestroy) {
                SessionAssistant.main.EventCounter();
                pu.jamType = chip.jamType;
                pu.DestroyChip();
            }
        }

        if (lightning != null) {
            lightning.Remove();
            if (currentSlot.chip) {
                currentSlot.chip.jamType = chip.jamType;
                currentSlot.chip.DestroyChip();
            }
        }

        branchCount --;
       
        while (branchCount > 0)
            yield return 0;

        yield return 0;
        branchCount = -1;


        //yield return new WaitForSeconds(0.02f);

        //for (int i = 0; i < branch.Count; i++) {
        //    yield return new WaitForSeconds(0.03f);
        //    if (branch[i].destroing || !branch[i].parentSlot)
        //        continue;

        //    branch[i].SetScore(0.1f);
        //    FieldAssistant.main.BlockCrush(branch[i].parentSlot.slot.x, branch[i].parentSlot.slot.y, true);
        //    FieldAssistant.main.JellyCrush(branch[i].parentSlot.slot.x, branch[i].parentSlot.slot.y);
        //    if (branch[i] != chip)
        //        branch[i].DestroyChip();
        //}

        //branchCount--;

        //while (branchCount > 0)
        //    yield return 0;

        //branchCount = -1;
    }

    int GetBranchCount() {
        return 6;
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip))
            return stack;
        foreach (Chip c in GameObject.FindObjectsOfType<Chip>())
            if (!c.destroying && c.slot)
                stack.Add(c);
        return stack;
    }

    #region Mixes
    public void RainbowMix(Chip secondary) {
        chip.destroying = true;
        StartCoroutine(DestroyChipFunction(secondary.chipType));
    }

    public string[] GetClipNames() {
        return new string[] { "Destroying", "Match" };
    }

    public string GetChipType() {
        return "RainbowHeart";
    }

    public bool IsMatchable() {
        return true;
    }

    public int GetPotencial() {
        return 1 + Slot.all.Count / 2;
    }
    #endregion
}