using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CustomEditor (typeof (DinaLabel))]
public class DinaLabelEditor : Editor {

    DinaLabel main;
    LocalizationAssistant assistant;
    List<string> mask_values;

    void OnEnable () {
        main = (DinaLabel) target;
        assistant = FindObjectOfType<LocalizationAssistant>();
        if (LocalizationAssistantEditor.content == null)
            LocalizationAssistantEditor.LoadContent();

        if (!DinaLabel.initialized)
            DinaLabel.Initialize();

        mask_values = DinaLabel.words.Keys.ToList();
        mask_values.Sort();
	}
	
	public override void OnInspectorGUI() {
        Undo.RecordObject(main, "DinaLabel changes");

        main.localized = EditorGUILayout.Toggle("Localized", main.localized);

        List<string> masks = new List<string>();

        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(300));
        EditorGUILayout.Space();

        if (main.localized) {
            #region Localized settings
            if (!assistant) {
                EditorGUILayout.HelpBox("Localization Assistant is missing", MessageType.Error);
            } else {
                int id = LocalizationAssistantEditor.keys.IndexOf(main.key);
                if (id < 0)
                    id = 0;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("key", GUILayout.Width(50));
                id = EditorGUILayout.Popup(id, LocalizationAssistantEditor.keys_menu);
                EditorGUILayout.EndHorizontal();

                main.key = LocalizationAssistantEditor.keys[id];

                foreach (SystemLanguage language in assistant.languages) {
                    EditorGUILayout.LabelField(language.ToString(), EditorStyles.miniLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Edit", GUILayout.Width(40)))
                        BerryPanel.CreateBerryPanel().EditLocalization(language, main.key);
                    if (LocalizationAssistantEditor.content.ContainsKey(language) && (LocalizationAssistantEditor.content[language].ContainsKey(main.key))) {
                        EditorGUILayout.HelpBox(LocalizationAssistantEditor.content[language][main.key], MessageType.None);
                        masks.AddRange(GetMasks(LocalizationAssistantEditor.content[language][main.key]));
                    }
                    else
                        EditorGUILayout.HelpBox("", MessageType.None);
                    EditorGUILayout.EndHorizontal();
                }
            }
            #endregion
        } else {
            #region Non localized settings
            main.text = EditorGUILayout.TextField("Text", main.text);
            masks.AddRange(GetMasks(main.text));
            #endregion
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        masks = masks.Distinct().ToList();
        masks.Sort();

        Dictionary<string, DinaLabel.Mask> _masks = main.masks.ToDictionary(x => x.key, x => x);

        foreach (string mask in masks) 
            if (!_masks.ContainsKey(mask))
                _masks.Add(mask, new DinaLabel.Mask(mask));

        if (_masks.Count > 0) {
            main.update = EditorGUILayout.Toggle("Update", main.update);
            GUI.enabled = main.update;
            main.delay = EditorGUILayout.Slider("Delay", main.delay, 0.1f, 3f);
            GUI.enabled = true;
        } else
            main.update = false;

        #region Masks panel
        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.MinWidth (300));
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Key", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
        GUILayout.Label("Value", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(200));

        EditorGUILayout.EndHorizontal();
        foreach (string key in _masks.Keys.ToArray()) {
            if (!masks.Contains(key))
                _masks.Remove(key);
            else {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(key, GUILayout.Width(100));

                int id = mask_values.IndexOf(_masks[key].value);
                if (id < 0) id = 0;

                id = EditorGUILayout.Popup(id, mask_values.ToArray(), GUILayout.Width(200));
                _masks[key].value = mask_values[id];

                EditorGUILayout.EndHorizontal();
            }
        }
        main.masks = _masks.Values.ToList();
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
        #endregion
    }

    List<string> GetMasks(string text) {
        List<string> result = new List<string>();
        if (string.IsNullOrEmpty(text))
            return result;
        int read_index = -1;
        foreach (char c in text) {
            if (c == '{') {
                read_index = result.Count;
                result.Add("");
                continue;
            }
            if (c == '}') {
                read_index = -1;
                continue;
            }
            if (read_index != -1)
                result[read_index] += c;
        }
        return result;
    }
}
