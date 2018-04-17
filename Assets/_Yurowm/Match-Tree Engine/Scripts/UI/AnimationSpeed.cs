using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Animation))]
public class AnimationSpeed : MonoBehaviour {

    public float speed = 1f;
    public float offset = 0f;
    public bool realTimeScale = false;
    Animation anim;
    string state;

	void Start () {
        anim = GetComponent<Animation>();
        state = anim.clip.name;
        anim[state].speed = speed;
        anim[state].time = offset * anim[state].length;
	}

    void Update() {
        if (!realTimeScale) return;
        anim.enabled = true;
        anim[state].time += Time.unscaledDeltaTime;
        anim.Sample();
        anim.enabled = false;
    }
}
