using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Berry.Utils;

// The base class for chips
public class Chip : MonoBehaviour, IAnimateChip {

    public static List<Chip> busyList = new List<Chip>();
    public static List<Chip> gravityBlockers = new List<Chip>();

    public static readonly int universalColorId = 10;
    public static readonly int uncoloredId = -1;

	public Slot slot; // Slot which include this chip
	public string chipType = "None"; // Chip type name
	public int id; // Chip color ID
	public int powerId; // Chip type ID
	public bool move = false; // is chip involved in the fall (SessionAssistant.main.gravity)
    public bool destroyable = true;
	public int movementID = 0;
	public Vector3 impulse = Vector3.zero;
    float velocity = 0;

    public string jamType = "";
    
    bool _busy = false;
    public bool busy {
        set {
            if (value == _busy)
                return;
            _busy = value;
            if (_busy)
                busyList.Add(this);
            else
                busyList.Remove(this);
        }

        get {
            return _busy;
        }
    }

    bool _gravity = true;
    public bool gravity {
        set {
            if (value == _gravity)
                return;
            _gravity = value;
            if (_gravity)
                gravityBlockers.Remove(this);
            else
                gravityBlockers.Add(this);
        }

        get {
            return _busy;
        }
    }

    public bool destroying = false; // in the process of destruction

    // Colors for each chip color ID
    public static readonly Color[] colors = {
		new Color(0.75f, 0.3f, 0.3f),
		new Color(0.3f, 0.75f, 0.3f),
		new Color(0.3f, 0.5f, 0.75f),
		new Color(0.75f, 0.75f, 0.3f),
		new Color(0.75f, 0.3f, 0.75f),
        new Color(0.75f, 0.5f, 0.3f),
    };


    public static readonly string[] chipTypes = {
                                           "Red",
                                           "Green",
                                           "Blue",
                                           "Yellow",
                                           "Purple",
                                           "Orange"
    };
	
	Vector3 lastPosition;
	Vector3 zVector;


    IChipLogic logic;

    public System.Action onPress = delegate { };
    public System.Action onUnpress = delegate { };

    void  Awake (){
        AnimationInitialize();

        logic = GetComponent<IChipLogic>();
        chipType = logic.GetChipType();

        Play("Awake");

        StartCoroutine(ChipPhysics());
	}

	// function of conditions of possibility of matching
	public bool IsMatcheble (){
        if (!logic.IsMatchable()) return false;
		if (IsUncolored()) return false;
		if (destroying) return false;
		if (busyList.Count == 0) return true;
		if (transform.position != slot.transform.position) return false;

		foreach (Side side in Utils.straightSides)
			if (slot[side]
			&& !slot[side].block
			&& !slot[side].GetShadow()
			&& !slot[side].chip)
				return false;

		return true;
	}

    void OnDrawGizmos() {
        if (busy) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + new Vector3(0.1f, 0.1f), transform.position - new Vector3(0.1f, 0.1f));
            Gizmos.DrawLine(transform.position + new Vector3(-0.1f, 0.1f), transform.position - new Vector3(-0.1f, 0.1f));
        }
        Gizmos.color = Color.green;
        if (slot)
            Gizmos.DrawLine(transform.position, slot.transform.position);
    }


    public bool IsUniversalColor() {
        return id == universalColorId;
    }

    public bool IsUncolored() {
        return id == uncoloredId;
    }

    public bool IsColored() {
        return id == Mathf.Clamp(id, 0, colors.Length - 1);
    }

    IEnumerator ChipPhysics() {
        while (true) {
            yield return 0;


            if (velocity > 0)
                velocity -= velocity * Mathf.Min(1f, Time.deltaTime * 3);

            if (!SessionAssistant.main.isPlaying || busy || destroying)
                continue;
            

            if (!slot) {
                if (!destroying)
                    DestroyChip();
                yield break;
            }

            #region Gravity
            while (transform.localPosition != Vector3.zero || impulse != Vector3.zero) {
                busy = true;
                if (destroying)
                    break;

                if (impulse == Vector3.zero) {
                    velocity += ProjectParameters.main.chip_acceleration * Time.deltaTime;
                    if (velocity > ProjectParameters.main.chip_max_velocity)
                        velocity = ProjectParameters.main.chip_max_velocity;

                    lastPosition = transform.position;

                    if (Mathf.Abs(transform.localPosition.x) < velocity * Time.deltaTime)
                        transform.localPosition = Utils.ScaleVector(transform.localPosition, 0, 1, 0);

                    if (Mathf.Abs(transform.localPosition.y) < velocity * Time.deltaTime)
                        transform.localPosition = Utils.ScaleVector(transform.localPosition, 1, 0, 0);

                    if (transform.localPosition.magnitude < 2 * Time.deltaTime) {
                        if (slot)
                            slot.slotGravity.GravityReaction();
                        if (transform.localPosition != Vector3.zero)
                            transform.position = lastPosition;
                        else {
                            busy = false;
                            movementID = SessionAssistant.main.GetMovementID();
                            velocity *= 0.5f;
                            OnHit();
                            break;
                        }
                    }

                    Vector3 moveVector = new Vector3();
                    if (transform.localPosition.x < 0)
                        moveVector.x = 1;
                    if (transform.localPosition.x > 0)
                        moveVector.x = -1;
                    if (transform.localPosition.y < 0)
                        moveVector.y = 1;
                    if (transform.localPosition.y > 0)
                        moveVector.y = -1;
                    moveVector = moveVector.normalized * velocity;
                    transform.localPosition += moveVector * Time.deltaTime;
                } else {
                    if (transform.localPosition.magnitude < ProjectParameters.main.slot_offset)
                        if (slot)
                            slot.slotGravity.GravityReaction();

                    if (impulse.sqrMagnitude > 4 * 4)
                        impulse = impulse.normalized * 4;

                    transform.position += impulse * Time.deltaTime;
                    transform.position -= transform.localPosition * Time.deltaTime;
                    impulse -= impulse * Time.deltaTime;
                    impulse -= transform.localPosition * 3f;
                    impulse *= Mathf.Max(0, 1f - Time.deltaTime * 6f);

                    if (impulse.magnitude < Time.deltaTime * 2) {
                        impulse = Vector3.zero;
                        busy = false;
                        break;
                    }
                }

                yield return 0;
                busy = false;
            }
            #endregion
        }
    }


	// returns the value of the potential of the current chips. needs for estimation of solution potential.
    public int GetPotencial() {
        int potential;
        Slot slot;
        switch (LevelProfile.main.target) {
            case FieldTarget.Jelly:
                potential = 1;
                foreach (Chip c in GetDangeredChips(new List<Chip>())) {
                    if (c.slot)
                        potential += c.slot.jelly ? 100 : 0;
                        foreach (Side side in Utils.straightSides) {
                            slot = Slot.GetSlot(c.slot.coord + side);
                            if (slot && slot.block)
                                potential += 5;
                        }
                }
                return potential;
            case FieldTarget.Block:
                potential = 1;
                foreach (Chip c in GetDangeredChips(new List<Chip>()))
                    foreach (Side side in Utils.straightSides) {
                        slot = Slot.GetSlot(c.slot.coord + side);
                        if (slot && slot.block)
                            potential += 10;
                    }
                return potential;
            case FieldTarget.Color:
                potential = 1;
                foreach (Chip c in GetDangeredChips(new List<Chip>()))
                    if (c.IsColored())
                        if (SessionAssistant.main.countOfEachTargetCount[c.id] > 0)
                            potential += 10;
                return potential;
            case FieldTarget.None:
                potential = 1;
                foreach (Chip c in GetDangeredChips(new List<Chip>())) {
                    potential += GetPotencial(c.powerId);
                    foreach (Side side in Utils.straightSides) {
                        slot = Slot.GetSlot(c.slot.coord + side);
                        if (slot && slot.block)
                            potential += 10;
                    }
                }
                return potential;
            case FieldTarget.SugarDrop:
                Slot s;
                potential = 1;
                int plus;
                foreach (Chip c in GetDangeredChips(new List<Chip>())) {
                    s = c.slot;
                    if (c.chipType == "Sugar")
                        continue;
                    plus = 0;
                    while (true) {
                        s = s[s.slotGravity.fallingDirection];

                        if (!s || !s.chip)
                            break;
                        else 
                            if (s.chip && s.chip.chipType == "Sugar")
                                plus ++;

                    }
                    if (plus > 0)
                        potential += plus * 10;
                    else
                        continue;
                    plus = 0;
                    s = c.slot;
                    while (true) {
                        if (!s)
                            break;
                        if (s.sugarDropSlot) {
                            plus = 1;
                            break;
                        }
                        s = s[s.slotGravity.gravityDirection];
                    }
                    if (plus == 1)
                        potential += 100;
                }

                return potential;
        }
        return 1;
		
	}

	// potential depending on powerID
	public int GetPotencial (int i){
        return logic.GetPotencial();
	}

    public void OnHit() {
        Play("Hit");
        AudioAssistant.Shot("ChipHit");
    }

	// separation of the chips from the parent slot
	public void  ParentRemove (){
		if (!slot) return;
		slot.chip = null;
		slot = null;
	}

    void OnDestroy() {
        busy = false;
        gravity = true;
    }

	// Starting the process of destruction of the chips
	public void  DestroyChip (){
        if (!destroyable) return;
		if (destroying) return;
		if (slot && slot.block) {
			slot.block.BlockCrush(false);
			return;
		}
		destroying = true;

        StartCoroutine(DestroyChipRoutine());
    }

    IEnumerator DestroyChipRoutine() {
        if (jamType == "")
            jamType = Jam.GetType(slot);
        Jam.JamIt(slot, jamType);

        yield return StartCoroutine(logic.Destroying());

        if (!destroying)
            yield break;


        if (IsColored())
            SessionAssistant.main.countOfEachTargetCount[id]--;


        Destroy(gameObject);
    }

	// Physically destroy the chip without activation and adding score points
	public void  HideChip (bool collection){
		if (destroying) return;
		destroying = true;

		ParentRemove();

        StartCoroutine(HidingRoutine());
	}

    IEnumerator HidingRoutine() {
        yield return StartCoroutine(MinimizingRoutine());
        Destroy(gameObject);
    }

    public void Minimize() {
        StartCoroutine(MinimizingRoutine());
    }

    IEnumerator MinimizingRoutine() {
        while (true) {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 6f);
            if (transform.localScale.x == 0) {
                yield break;
            }
            yield return 0;
        }
    }

    // Adding score points
    public void  SetScore (float s){
		SessionAssistant.main.score += Mathf.RoundToInt(s * SessionAssistant.scoreC);
		ScoreBubble.Bubbling(Mathf.RoundToInt(s * SessionAssistant.scoreC), transform, id);
	}

	// To begin the process of flashing (for hints - SessionAssistant.main.ShowHint)
	public void Flashing (int eventCount){
		StartCoroutine (FlashingUntil (eventCount));
	}

	// Coroutinr of flashing chip until a specified count of events
	IEnumerator  FlashingUntil (int eventCount){
        Play("Flashing");
		while (eventCount == SessionAssistant.main.eventCount) yield return 0;
		if (!this) yield break;
        Complete("Flashing");
	}

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(this))
            return stack;

        stack = logic.GetDangeredChips(stack);
        return stack;
    }

    public static void Swap(Chip chip, Side side) {
        if (chip.slot && chip.slot[side])
            SessionAssistant.main.SwapByPlayer(chip, chip.slot[side].chip, false);
    }

    public void Reset() {
        transform.localScale = Vector3.one;
        movementID = 0;
        impulse = Vector3.zero;
        destroying = false;
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
    }

    #region Animations
    string[] IAnimateChip.GetClipNames() {
        return new string[] { "Flashing", "Awake"
            //, "Hit"
        };
    }

    public List<Clip> clips_serialized = new List<Clip>();
    public void Play(string clip_name) {
        if (clips.ContainsKey(clip_name))
            anim.Play(clip_name);
    }

    public void Stop(string clip_name) {
        if (clips.ContainsKey(clip_name))
            anim.Stop(clip_name);
    }

    public void Complete(string clip_name) {
        if (clips.ContainsKey(clip_name))
            StartCoroutine(CompleteRoutine(clip_name));
    }

    public bool IsPlaying(string clip_name) {
        if (clips.ContainsKey(clip_name))
            return anim.IsPlaying(clip_name);
        return false;
    }

    public bool IsPlaying() {
        return anim.isPlaying;
    }

    IEnumerator CompleteRoutine(string clip_name) {
        while (anim[clip_name].time % anim[clip_name].length > 0.1f)
            yield return 0;
        anim[clip_name].time = 0;
        yield return 0;
        anim.Stop(clip_name);
    }

    Animation anim;
    public Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
    void AnimationInitialize() {
        anim = GetComponent<Animation>();
        foreach (Clip clip in clips_serialized)
            if (clip.clip != null)
                clips.Add(clip.name, clip.clip);
        foreach (KeyValuePair<string, AnimationClip> pair in clips)
            if (pair.Value != null)
                anim.AddClip(pair.Value, pair.Key);
    }
    #endregion
}

public class IBomb : MonoBehaviour {

}

public interface IAnimateChip {
    string[] GetClipNames();
}

public interface IChipLogic {
    IEnumerator Destroying();
    string GetChipType();
    List<Chip> GetDangeredChips(List<Chip> stack);
    bool IsMatchable();
    int GetPotencial();

    Chip chip { get; }
}

[System.Serializable]
public class Clip {
    public string name;
    public AnimationClip clip;

    public Clip (string _name) {
        name = _name;
    }

    public static bool operator ==(Clip a, Clip b) {
        return a.Equals(b);
    }

    public static bool operator !=(Clip a, Clip b) {
        return !a.Equals(b);
    }

    public override bool Equals(object obj) {
        if (obj == null || !(obj is Clip))
            return false;
        return name == ((Clip) obj).name;
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
}