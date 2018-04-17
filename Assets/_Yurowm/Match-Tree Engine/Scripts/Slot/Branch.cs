using UnityEngine;
using System.Collections;

public class Branch : IBlock {

	int eventCountBorn = 0;
	bool destroying = false;
	SlotGravity gravity;
	
	void Start () {
		transform.rotation = Quaternion.Euler (0, 0, Random.Range (0f, 360f));	
	}

	#region implemented abstract members of BlockInterface

    public override void BlockCrush(bool force) {
		if (eventCountBorn == SessionAssistant.main.eventCount && !force) return;
		if (destroying) return;
		eventCountBorn = SessionAssistant.main.eventCount;
		GameObject o = ContentAssistant.main.GetItem ("BranchCrush");
		o.transform.position = transform.position;
        slot.SetScore(1);
		SlotGravity.Reshading();
		Destroy(gameObject);
		return;
	}

	public override bool CanBeCrushedByNearSlot () {
		return false;
	}

    public override bool CanItContainChip() {
        return true;
    }

    public override int GetLevels() {
        return 1;
    }
	#endregion

	override public void Initialize () {
		gravity = slot.GetComponent<SlotGravity> ();
		gravity.enabled = false;
		eventCountBorn = SessionAssistant.main.eventCount;
	}

	void OnDestroy() {
		gravity.enabled = true;
	}

}
