using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;
using EditorUtils;

[CustomEditor (typeof (ContentAssistant))]
public class ContentAssistantEditor : MetaEditor {

	private ContentAssistant main;
	private GameObject obj = null;
	private string category = "";
    private Dictionary<string, AnimBool> categories = new Dictionary<string, AnimBool>();

	public override void OnInspectorGUI () {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("ContentAssistant is missing", MessageType.Error);
            return;
        }
        main = (ContentAssistant) metaTarget;
        Undo.RecordObject(main, "");

		if (main.cItems == null)
            main.cItems = new List<ContentAssistant.ContentAssistantItem> ();

        foreach (ContentAssistant.ContentAssistantItem i in main.cItems)
            if (!categories.ContainsKey(i.category)) {
                categories.Add(i.category, new AnimBool(false));
                categories[i.category].valueChanged.AddListener(RepaintIt);
            }

        foreach (var key in categories.Keys) {

            categories[key].target = GUILayout.Toggle(categories[key].target, key, EditorStyles.foldout);

            if (EditorGUILayout.BeginFadeGroup(categories[key].faded)) {
                foreach (ContentAssistant.ContentAssistantItem j in main.cItems.FindAll(x => x.category == key)) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button("X", GUILayout.Width(20))) {
                        obj = j.item;
                        this.category = j.category;
                        main.cItems.Remove(j);
                        return;
                    }
                    GameObject _obj = (GameObject) EditorGUILayout.ObjectField(j.item, typeof(GameObject), false, GUILayout.Width(250));
                    if (j.item != _obj)
                        main.cItems[main.cItems.IndexOf(j)] = new ContentAssistant.ContentAssistantItem(_obj, key);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        #region Add item
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add", GUILayout.Width(50))) {
            if (obj == null || category == null)
                return;
            if (category == "")
                category = "Other";
            main.cItems.Add(new ContentAssistant.ContentAssistantItem(obj, category));
            if (!categories.ContainsKey(category))
                categories.Add(category, new AnimBool(true));
            else
                categories[category].target = true;
            obj = null;
            category = "";
        }
        obj = (GameObject) EditorGUILayout.ObjectField(obj, typeof(GameObject), false, GUILayout.Width(150));
        GUILayout.Label("in", GUILayout.Width(30));
        category = GUILayout.TextField(category, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();
        #endregion
    }

    public override Object FindTarget() {
        if (ContentAssistant.main == null)
            ContentAssistant.main = FindObjectOfType<ContentAssistant>();
        return ContentAssistant.main;
    }
}
