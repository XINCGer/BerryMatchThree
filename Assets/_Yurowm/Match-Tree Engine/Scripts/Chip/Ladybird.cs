using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Berry.Utils;

[RequireComponent(typeof(Chip))]
public class Ladybird : IBomb, IAnimateChip, IChipLogic {
    public static List<Slot> targetStack = new List<Slot>();

    Chip _chip;
    int birth; // Event count at the time of birth SessionAssistant.main.eventCount
    int branchCount;

    public Transform directionSprite;

    Slot target;
    public string seed = "";

    public Chip chip {
        get {
            return _chip;
        }
    }

    void OnDestroy() {
        if (target != null && targetStack.Contains(target)) {
            targetStack.Remove(target);
        }
    }

    void Awake() {
        _chip = GetComponent<Chip>();
        birth = SessionAssistant.main.eventCount;
        AudioAssistant.Shot("LadybirdCreate");
    }

    // Coroutine destruction / activation
    public IEnumerator Destroying() {
        if (birth == SessionAssistant.main.eventCount) {
            chip.destroying = false;
            yield break;
        }


        chip.busy = true;
        chip.gravity = false;

        chip.Play("Flying");
        AudioAssistant.Shot("LadybirdCreate");

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            sr.sortingLayerName = "Foreground";

        if (chip.slot) {
            FieldAssistant.main.BlockCrush(chip.slot.coord, false);
            FieldAssistant.main.JellyCrush(chip.slot.coord);
        }
        
        chip.ParentRemove();

        Vector3 startPosition = chip.transform.position;
        Vector3 lastPosition = transform.position;
        Vector3 tagetPosition;

        if (!target)
            target = FindTarget();

        float speed = Random.Range(3f, 4f) / Vector3.Distance(startPosition, target.transform.position);

        Vector3 normal = (target.transform.position - startPosition).normalized;
        normal = new Vector3(-normal.y, normal.x, 0);
        normal *= Vector3.Distance(target.transform.position, startPosition) * Random.Range(0.2f, 0.4f);
        if (Random.value > 0.5f)
            normal *= -1;

        float time = 0f;
        float angle = 0;
        while (time < 1) {
            time += Time.deltaTime * speed;

            tagetPosition = target.transform.position + normal * Mathf.Sin(Mathf.PI * time);
            transform.position = Vector3.Lerp(startPosition, tagetPosition, EasingFunctions.easeInOutQuad(time));

            angle = Vector3.Angle(directionSprite.up, transform.position - lastPosition);
            if (Vector3.Angle(-directionSprite.right, transform.position - lastPosition) > 90)
                angle *= -1;

            directionSprite.Rotate(0, 0, angle * Time.deltaTime * 15);
            
            lastPosition = transform.position;
            yield return 0;
        }

        SessionAssistant.main.EventCounter();

        Slot _target = target;
        if (seed != "") {
            Chip pu = FieldAssistant.main.AddPowerup(_target.coord, seed);
            pu.busy = true;
            _target.chip = pu;
            yield return 0;
            SessionAssistant.main.EventCounter();
        }
        Crush(_target.coord);
        chip.busy = false;
        chip.gravity = true;

        AudioAssistant.Shot("LadybirdCrush");
        SessionAssistant.main.EventCounter();

        targetStack.Remove(target);

        yield return new WaitForSeconds(0.1f);

        chip.Play("Destroying");
        AnimationAssistant.main.Explode(transform.position, 5, 7);

        while (chip.IsPlaying("Destroying"))
            yield return 0;
        
        Destroy(gameObject);
    }

    Slot FindTarget() {
        Slot result;
        switch (LevelProfile.main.target) {
            case FieldTarget.Block:
                Block[] blocks = 
                    Slot.all.Values.Where(x => x.block != null && x.block is Block && !targetStack.Contains(x))
                    .Select(x => x.block as Block).ToArray();
                if (blocks.Length > 0) {
                    result = blocks[Random.Range(0, blocks.Length)].slot;
                    targetStack.Add(result);
                    return result;
                }
                break;
            case FieldTarget.Color:
            case FieldTarget.Jelly:
            case FieldTarget.None:
            case FieldTarget.Jam:
            case FieldTarget.SugarDrop: {
                    List<Chip> chips = new List<Chip>(FindObjectsOfType<Chip>());
                    chips = chips.FindAll(x => !x.busy).ToList();
                    int potential = -1;
                    int z = 0;
                    List<Chip> resultChip = new List<Chip>();
                    foreach (Chip c in chips) {
                        if (c.chipType == "Ladybird" || !c.destroyable)
                            continue;
                        if (c.destroying || !c.slot || targetStack.Contains(c.slot))
                            continue;
                        z = c.GetPotencial();
                        if (potential < z) {
                            resultChip.Clear();
                            potential = z;
                        }
                        if (potential == z)
                            resultChip.Add(c);
                    }
                    if (chip.jamType != "")
                        resultChip = resultChip.FindAll(x => Jam.GetType(x.slot) != chip.jamType).ToList();
                    if (resultChip.Count > 0) {
                        result = resultChip[Random.Range(0, resultChip.Count)].slot;
                        targetStack.Add(result);
                        return result;
                    }
                    break;
                }

        }

        Slot[] targets = Slot.all.Values.Where(x => !targetStack.Contains(x) && x != chip.slot && x.chip != null).ToArray();
        result = targets[Random.Range(0, targets.Length)];
        targetStack.Add(result);
        return result;
    }

    bool Crush(int2 coord) {
        Slot s = Slot.GetSlot(coord);
        FieldAssistant.main.BlockCrush(coord, false);
        FieldAssistant.main.JellyCrush(coord);
        if (s && s.chip) {
            s.chip.SetScore(3f);
            s.chip.jamType = chip.jamType;
            s.chip.DestroyChip();
            AnimationAssistant.main.Explode(s.transform.position, 3, 7);
        }
        return coord.IsItHit(0, 0, LevelProfile.main.width - 1, LevelProfile.main.height - 1);
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip))
            return stack;

        stack.Add(chip);
        return stack;
    }

    #region Mixes
    public void LadybirdsMix(Chip secondary) {
        StartCoroutine(LadybirdsMixRoutine(secondary));
    }

    IEnumerator LadybirdsMixRoutine(Chip secondary) {
        chip.busy = true;
        chip.destroyable = false;

        yield return 0;

        List<Chip> ladies = new List<Chip>();
        int count = 3;
        if (secondary.chipType == "Ladybird")
            count = 5;
        if (chip.slot.jam)
            chip.jamType = chip.slot.jam.type;
        for (int i = 0; i <= count; i++) {
            Chip l = ContentAssistant.main.GetItem<Chip>("Ladybird" + Chip.chipTypes[Random.value > 0.5f ? chip.id : secondary.id]);
            l.destroyable = false;
            l.transform.position = chip.slot.transform.position;
            l.transform.localScale = Vector3.one;
            l.transform.SetParent(Slot.folder);
            l.transform.Find("LadybirdBody").rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            l.jamType = chip.jamType;
            if (secondary.chipType != "Ladybird")
                l.GetComponent<Ladybird>().seed = secondary.chipType;
            ladies.Add(l);
        }

        chip.Minimize();

        SessionAssistant.main.EventCounter();
        foreach (Chip l in ladies) {
            if (l == null) continue;
            Animation a = l.GetComponent<Animation>();
            while (a.isPlaying)
                yield return 0;
            l.destroyable = true;
            l.jamType = chip.jamType;
            l.DestroyChip();
        }


        chip.busy = false;
        chip.HideChip(false);
    }

    public string[] GetClipNames() {
        return new string[] { "Destroying", "Flying" };
    }

    public string GetChipType() {
        return "Ladybird";
    }

    public bool IsMatchable() {
        return true;
    }

    public int GetPotencial() {
        return 15;
    }
    #endregion

}