using UnityEngine;
using System.Collections;

// The effect of the particles. Upon completion will be removed.
public class ParticleEffect : MonoBehaviour {
	
	ParticleSystem ps;
	public string sortingLayer;
	public int sortingOrder;
	public bool killAfterLifetime = true;
	
	void  Start (){
		ps = GetComponent<ParticleSystem>();
		ps.GetComponent<Renderer>().sortingLayerName = sortingLayer;
		ps.GetComponent<Renderer>().sortingOrder = sortingOrder;
		if (killAfterLifetime) StartCoroutine("Kill");

        if (transform.parent == null)
            transform.parent = GameObject.Find("Slots").transform;
	}
	
	IEnumerator Kill (){
		yield return new WaitForSeconds(ps.duration + ps.startLifetime);
		Destroy(gameObject);
	}
}