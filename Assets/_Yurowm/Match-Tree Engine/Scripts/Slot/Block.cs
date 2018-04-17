using UnityEngine;
using System.Collections;

// Destroyable blocks on playing field
public class Block : IBlock {
    
	public Sprite[] sprites; // Images of blocks of different levels. The size of the array must be equal to 3
	SpriteRenderer sr;
	int eventCountBorn;
    Animation anim;
    bool destroying = false;
    public string crush_effect;


	
	#region implemented abstract members of IBlock
    override public void Initialize (){
		sr = GetComponent<SpriteRenderer>();
		eventCountBorn = SessionAssistant.main.eventCount;
		sr.sprite = sprites[level-1];
        anim = GetComponent<Animation>();
	}
	
	// Crush block funtion
	override public void  BlockCrush (bool force) {
        if (destroying)
            return;
		if (eventCountBorn == SessionAssistant.main.eventCount && !force) return;
		eventCountBorn = SessionAssistant.main.eventCount;
		level --;
        if (level == 0) {
            slot.SetScore(1);
            slot.block = null;
            SlotGravity.Reshading();
            StartCoroutine(DestroyingRoutine());
			return;
		}
		if (level > 0) {
			anim.Play("BlockCrush");
            AudioAssistant.Shot("BlockHit");
			sr.sprite = sprites[level-1];
		}
	}

	public override bool CanBeCrushedByNearSlot () {
		return true;
	}

    public override bool CanItContainChip() {
        return false;
    }

    public override int GetLevels() {
        return 3;
    }
	#endregion

    IEnumerator DestroyingRoutine() {
        destroying = true;

        GameObject o = ContentAssistant.main.GetItem(crush_effect);
        o.transform.position = transform.position;

        anim.Play("BlockDestroy");
        AudioAssistant.Shot("BlockCrush");
        while (anim.isPlaying) {
            yield return 0;
        }

        Destroy(gameObject);
    }

}