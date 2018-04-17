using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Jelly element on playing field
public class Jelly : MonoBehaviour {

	public int level = 1; // Level of jelly. From 1 to 3. Each "JellyCrush"-call fall level by one. If it becomes zero, this jelly will be destroyed.
	public Sprite[] sprites; // Images of jellies of different levels. The size of the array must be equal to 3
	SpriteRenderer sr;
    Animation anim;
    public string crush_effect;
    
    void Start() {
		sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[level - 1];
        anim = GetComponent<Animation>();
        AnimationSpeed speed = GetComponentInChildren<AnimationSpeed>();
        speed.speed = Random.Range(0.4f, 0.8f);
        speed.offset = Random.Range(0f, 1f);
	}

	// Crush block funtion
	public void JellyCrush (){
        if (level == 1) {
            AudioAssistant.Shot("JellyCrush");
            StartCoroutine(DestroyingRoutine());
			return;
		}
        level--;
        anim.Play("JellyCrush");
        AudioAssistant.Shot("JellyrHit");
		sr.sprite = sprites[level-1];
	}

    IEnumerator DestroyingRoutine() {
        GameObject o = ContentAssistant.main.GetItem(crush_effect);
        o.transform.position = transform.position;

        anim.Play("BlockDestroy");
        while (anim.isPlaying)
            yield return 0;

        Destroy(gameObject);
    }
}