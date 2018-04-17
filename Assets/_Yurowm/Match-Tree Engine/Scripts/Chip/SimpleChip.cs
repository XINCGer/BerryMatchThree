using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

// The class is responsible for logic SimpleChip
[RequireComponent (typeof (Chip))]
public class SimpleChip : MonoBehaviour, IAnimateChip, IChipLogic {

	Chip _chip;
    public Chip chip {
        get {
            if (_chip == null)
                _chip = GetComponent<Chip>();
            return _chip;
        }
	}

    public Chip GetChip() {
        return chip;
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        stack.Add(chip);
        return stack;
    }

    public string GetChipType() {
        return "SimpleChip";
    }

    public int GetPotencial() {
        return 1;
    }

    public bool IsMatchable() {
        return true;
    }

	public IEnumerator Destroying (){
		chip.busy = true;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
		AudioAssistant.Shot("ChipCrush");
		
		yield return new WaitForSeconds(0.1f);
		
		chip.ParentRemove();
        chip.busy = false;

        if (chip.IsColored() && SessionAssistant.main.countOfEachTargetCount[chip.id] > 0) {
            GameObject go = GameObject.Find("ColorTargetItem" + Chip.chipTypes[chip.id]);

            if (go) {
                Transform target = go.transform;
                
                sprite.sortingLayerName = "UI";
                sprite.sortingOrder = 10;

                float time = 0;
                float speed = Random.Range(1f, 1.8f);
                Vector3 startPosition = transform.position;
                Vector3 targetPosition = target.position;

                while (time < 1) {
                    transform.position = Vector3.Lerp(startPosition, targetPosition, EasingFunctions.easeInOutQuad(time));
                    time += Time.unscaledDeltaTime * speed;
                    yield return 0;
                }

                transform.position = target.position;
            }
        }         

        chip.Play("Destroying");

        while (chip.IsPlaying("Destroying"))
            yield return 0;

        Destroy(gameObject);
	}

    public string[] GetClipNames() {
        return new string[] { "Destroying" };
    }

}