using UnityEngine;
using System.Collections;

public class SetSortingLayer : MonoBehaviour {

	public enum SortingLayerType {Mesh, Particle, Trail};
	public SortingLayerType type = SortingLayerType.Mesh;
	public string sortingLayerName;
	public int sortingOrder = 0;
	
	void Start () {
		Refresh ();
	}

    [ContextMenu("Refresh")]
    public void Refresh() {
        switch (type) {
            case SortingLayerType.Mesh:
                GetComponent<Renderer>().sortingLayerName = sortingLayerName;
                GetComponent<Renderer>().sortingOrder = sortingOrder;
                break;
            case SortingLayerType.Particle:
                GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = sortingLayerName;
                GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingOrder = sortingOrder;
                break;
            case SortingLayerType.Trail:
                TrailRenderer trail = GetComponent<TrailRenderer>();
                trail.sortingLayerName = sortingLayerName;
                trail.sortingOrder = sortingOrder;
                break;
        }
    }

}