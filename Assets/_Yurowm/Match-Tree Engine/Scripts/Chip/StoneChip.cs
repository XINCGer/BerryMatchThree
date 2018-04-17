using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (Chip))]
public class StoneChip : MonoBehaviour, IAnimateChip, IChipLogic {
	
	public Chip _chip;

    public Chip chip {
        get {
            return _chip;
        }
    }

    public string GetChipType() {
        return "Stone";
    }

    public string[] GetClipNames() {
        return new string[] { "Destroying" };
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        stack.Add(chip);
        return stack;
    }

    public int GetPotencial() {
        return 1;
    }

    public bool IsMatchable() {
        return false;
    }

    void  Awake (){
		_chip = GetComponent<Chip>();
	}

    // Coroutine destruction / activation
    public IEnumerator Destroying() {

        chip.busy = true;
        AudioAssistant.Shot("StoneCrush");
		chip.Play("Destroying");
		
		yield return new WaitForSeconds(0.1f);
		chip.busy = false;
		
        chip.SetScore(1);

		chip.ParentRemove();

		GameObject o = ContentAssistant.main.GetItem ("StoneCrush");
		o.transform.position = transform.position;

		while (chip.IsPlaying("Destroying"))
            yield return 0;
		Destroy(gameObject);
	}
}