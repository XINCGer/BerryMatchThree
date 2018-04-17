using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

public class Weed : IBlock {

	public static List<Weed> all = new List<Weed>();

	int eventCountBorn;

	bool destroying = false;

    Animation anim;
    public string crush_effect;

	public static int seed = 0;
    public static int lastWeedCrush = 0;

	void Start () {
        anim = GetComponent<Animation>();
    }

	override public void Initialize (){
		eventCountBorn = SessionAssistant.main.eventCount;
		all.Add (this);
	}	

	#region implemented abstract members of BlockInterface
	
	// Crush block funtion
    override public void BlockCrush(bool force) {
		if (eventCountBorn == SessionAssistant.main.eventCount && !force) return;
		if (destroying) return;

        lastWeedCrush = SessionAssistant.main.swapEvent;

		eventCountBorn = SessionAssistant.main.eventCount;

        slot.SetScore(1);
        StartCoroutine(DestroyingRoutine());
	}

	public override bool CanBeCrushedByNearSlot () {
		return true;
	}

    public override bool CanItContainChip() {
        return false;
    }

    public override int GetLevels() {
        return 1;
    }
    #endregion

    void OnDestroy () {
		all.Remove (this);
	}

	public static void Grow () {
		List<Slot> slots = new List<Slot> ();

		foreach (Weed weed in all)
			foreach (Side side in Utils.straightSides)
                if (weed.slot[side] && !weed.slot[side].block && !(weed.slot[side].chip && weed.slot[side].chip.chipType == "Sugar"))
					slots.Add(weed.slot[side]);

        while (seed > 0) {
		    if (slots.Count == 0) return;
            
		    Slot target = slots.GetRandom();
            slots.Remove(target);

		    if (target.chip)
			    target.chip.HideChip(false);

		    Weed newWeed = ContentAssistant.main.GetItem<Weed>("Weed");
		    newWeed.transform.position = target.transform.position;
		    newWeed.name = "New_Weed";
		    newWeed.transform.parent = target.transform;
		    target.block = newWeed;
		    newWeed.slot = target;
            AudioAssistant.Shot("WeedCreate");
            newWeed.Initialize();

            seed--;
        }

	}

    IEnumerator DestroyingRoutine() {
        destroying = true;

        GameObject o = ContentAssistant.main.GetItem(crush_effect);
        o.transform.position = transform.position;

        AudioAssistant.Shot("WeedCrush");
        anim.Play("JellyDestroy");
        while (anim.isPlaying) {
            yield return 0;
        }

        slot.block = null;
        SlotGravity.Reshading();
        Destroy(gameObject);
    }

}