using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Prefab Storage
public class ContentAssistant : MonoBehaviour {

	public static ContentAssistant main;

	// List prefabs with categories
	public List<ContentAssistantItem> cItems;

	// Dictionary prefabs for quick retrieval
	Dictionary<string, GameObject> content = new Dictionary<string, GameObject>();

	GameObject zObj;    

    Transform stack;

	void Awake () {
        if (main == null)
            Initialize();

        stack = new GameObject().transform;
        stack.name = "Stack";
        stack.parent = transform;
	}

    public void Initialize() {
		main = this;
		content.Clear ();
		foreach (ContentAssistantItem item in cItems)
			content.Add(item.item.name, item.item);
    }

    // functions to instantiate prefabs

    public T GetItem<T> (string key) where T : Component {
        GameObject obj = GetItem(key);
        if (obj)
            return obj.GetComponent<T>();
        return null;
	}

    public T GetPrefab<T>(string key) where T : Component {
        GameObject obj = GetPrefab(key);
        if (obj)
            return obj.GetComponent<T>();
        return null;
    }

    public GameObject GetItem (string key) {
        if (content.ContainsKey(key))
            return Instantiate(content[key]);
        return null;
	}

    public GameObject GetPrefab(string key) {
        if (content.ContainsKey(key))
            return content[key];
        return null;
    }

    public T GetItem<T> (string key, Vector3 position) where T : Component {
		zObj = GetItem (key);
		zObj.transform.position = position;
		return zObj.GetComponent<T>();
	}

	public GameObject GetItem(string key, Vector3 position) {
		zObj = GetItem (key);
		zObj.transform.position = position;
		return zObj;
	}

	public GameObject GetItem(string key, Vector3 position, Quaternion rotation) {
		zObj = GetItem (key, position);
		zObj.transform.rotation = rotation;
		return zObj;
	}

	[System.Serializable]
	public struct ContentAssistantItem {
		public GameObject item;
		public string category;

        public ContentAssistantItem(GameObject _item, string _category) : this() {
            item = _item;
            category = _category;
        }
    }
}