using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SugarChip : MonoBehaviour, IAnimateChip, IChipLogic {

    public Chip _chip;

    bool _live = false;
    public static int live_count = 0;
    public bool live {
        set {
            if (value == _live)
                return;
            _live = value;
            if (_live)
                live_count++;
            else
                live_count--;
        }
        get {
            return _live;
        }
    }

    public Chip chip {
        get {
            return _chip;
        }
    }

    void OnDestroy() {
        live = false;
    }

    void Awake() {
        _chip = GetComponent<Chip>();
        live = true;
        chip.destroyable = false;
        AudioAssistant.Shot("SugarCreate");
    }

    void Update() {
        if (chip.destroying) return;
        if (!chip.slot) return;
        if (!SessionAssistant.main.CanIWait()) return;
        if (chip.slot.sugarDropSlot && transform.localPosition == Vector3.zero) {
            chip.destroyable = true;
            SessionAssistant.main.targetSugarDropsCount++;
            chip.DestroyChip();
        }
    }

    // Coroutine destruction / activation
    public IEnumerator Destroying() {

        chip.busy = true;
        AudioAssistant.Shot("SugarCrush");

        yield return new WaitForSeconds(0.2f);
        chip.busy = false;

        chip.ParentRemove();


        float velocity = 0;
        Vector3 impuls = new Vector3(Random.Range(-3f, 3f), Random.Range(1f, 5f), 0);
        impuls += chip.impulse;
        chip.impulse = Vector3.zero;
        foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
            sprite.sortingLayerName = "UI";


        float rotationSpeed = Random.Range(-30f, 30f);
        float growSpeed = Random.Range(0.2f, 0.8f);

        while (transform.position.y > -10) {
            velocity += Time.deltaTime * 20;
            velocity = Mathf.Min(velocity, 40);
            transform.position += impuls * Time.deltaTime * transform.localScale.x;
            transform.position -= Vector3.up * Time.deltaTime * velocity * transform.localScale.x;
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
            yield return 0;
        }

        Destroy(gameObject);
    }

    public string[] GetClipNames() {
        return new string[0];
    }

    public string GetChipType() {
        return "Sugar";
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        stack.Add(chip);
        return stack;
    }

    public bool IsMatchable() {
        return false;
    }

    public int GetPotencial() {
        return 0;
    }
}
