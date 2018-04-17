using UnityEngine;
using System.Collections;

// Lightning effect
public class Lightning: MonoBehaviour {
	
	public Transform start; // start object
	public Transform end; //end object
	public int bend = 2; // bends count
	public Vector3[] bendPoint; // points of bending
	public Color color; // lightning color
	
	public string sortingLayer;
	public int sortingOrder;

	LineRenderer line;
	float distance = 0f;
	float lastTime = -100f;
	float frequency = 20f;
	bool  destroing = false;
	Vector3 a;
	Vector3 b;
	
	
	void  Start (){
		line = GetComponent<LineRenderer>();
		bendPoint = new Vector3[bend];
		line.SetColors(color, color);
		line.SetVertexCount(bend + 2);
		line.GetComponent<Renderer>().sortingLayerName = sortingLayer;
		line.sortingOrder = sortingOrder;

        transform.parent = GameObject.Find("Slots").transform;
	}
	
	void  Update (){
		if (end == null || !end.gameObject.activeSelf || start == null || !start.gameObject.activeSelf) {
            Remove();
            return;
		}
		
		if (!destroing) {
			a = start.position;
			b = end.position;
		}
		distance = (a - b).magnitude;
		if (lastTime + 1f/frequency < Time.time) {
			lastTime = Time.time;
			for (int i = 0; i < bendPoint.Length; i++)
				bendPoint[i] = new Vector3((2f * Random.value - 1f) * 0.1f * distance, (2f * Random.value - 1f) * 0.1f * distance, 0f);
		}
		line.SetPosition(0, a);
		for (int i= 1; i < bend + 1; i++) {
			line.SetPosition(i, Vector3.Lerp(a, b, (1f * i)/(bend+1)) + bendPoint[i-1]);
		}
		line.SetPosition(bend + 1, b);
	}

    public void Remove() {
        StartCoroutine(FadeOut());
    }

	IEnumerator FadeOut (){
		if (destroing) yield break;
		destroing = true;
		while (GetComponent<Animation>().isPlaying) yield return 0;
		GetComponent<Animation>().Play("LightningFadeOut");
		while (GetComponent<Animation>().isPlaying) yield return 0;
		Destroy(gameObject);
	}

	// function of creating new lightning effect
	public static Lightning CreateLightning (int bend, Transform start, Transform end, Color color) {
		Lightning newLightning = ContentAssistant.main.GetItem<Lightning> ("Lightning");
		newLightning.bend = bend;
		newLightning.start = start;
		newLightning.end = end;
		newLightning.color = color;
		return newLightning;
	}
}