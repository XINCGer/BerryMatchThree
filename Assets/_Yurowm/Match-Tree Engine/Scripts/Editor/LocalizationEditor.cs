using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System;
using EditorUtils;

public class LocalizationEditor : MetaEditor {

    public static Dictionary<string, string> phrases = new Dictionary<string, string>();
    public SystemLanguage? language = null;

    public Action<Vector2> scroll = delegate { };

    string new_key = "";
    public string search = "";
    GUIStyle searchStyle, searchXStyle;
    public void OnEnable() {
        if (language.HasValue) {
            LocalizationAssistantEditor.LoadContent();
            phrases = LocalizationAssistantEditor.content[language.Value];
        }
    }

    public override void OnInspectorGUI () {
        if (!metaTarget)
            return;

        if (language == null) {
            language = LocalizationAssistant.main.languages[0];
            OnEnable();
        }

        if (searchStyle == null)
            searchStyle = GUI.skin.FindStyle("ToolbarSeachTextField");
        if (searchXStyle == null)
            searchXStyle = GUI.skin.FindStyle("ToolbarSeachCancelButton");

        bool changed = false;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Localization table", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));

        #region Toolbar
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

        List<string> languages = LocalizationAssistant.main.languages.Select(x => x.ToString()).ToList();
        int id = languages.IndexOf(language.ToString());
        int _id = EditorGUILayout.Popup(id, languages.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(100));
        if (id != _id) {
            language = (SystemLanguage) Enum.Parse(typeof (SystemLanguage), languages[_id]);
            OnEnable();
        }

        GUILayout.FlexibleSpace();

        search = EditorGUILayout.TextField(search, searchStyle, GUILayout.Width(120));
        if (GUILayout.Button("", searchXStyle)) {
            search = "";
            EditorGUI.FocusTextInControl("");
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60))) {
            LocalizationAssistantEditor.LoadContent();
            phrases = LocalizationAssistantEditor.content[language.Value];
        }

        if (GUILayout.Button("Sort", EditorStyles.toolbarButton, GUILayout.Width(50))) {
            phrases = phrases.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            changed = true;
        }

        if (GUILayout.Button("Get Missed Keys", EditorStyles.toolbarButton, GUILayout.Width(110))) {
            foreach (string key in LocalizationAssistantEditor.keys)
                if (!phrases.ContainsKey(key)) {
                    phrases.Add(key, "");
                    changed = true;
                    scroll(Vector2.up * 1000000);
                }
            Type target_interface = typeof(INeedLocalization);
            foreach (Type type in (typeof(LocalizationAssistant)).Assembly.GetTypes())
                if (type != target_interface && target_interface.IsAssignableFrom(type)) 
                    foreach (var comp in FindObjectsOfType(type))
                        foreach (string key in (comp as INeedLocalization).RequriedLocalizationKeys())
                            if (!phrases.ContainsKey(key)) {
                                phrases.Add(key, "");
                                changed = true;
                                scroll(Vector2.up * 1000000);
                            }
            
        }

        EditorGUILayout.EndHorizontal();
        #endregion

        #region Add new key
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = new_key != "" && !phrases.ContainsKey(new_key);
        if (GUILayout.Button("Add new key", EditorStyles.toolbarButton, GUILayout.Width(90))) {
            phrases.Add(new_key, "");
            changed = true;
            new_key = "";
            scroll(Vector2.up * 1000000);
            EditorGUI.FocusTextInControl("");
        }
        GUI.enabled = true;
        new_key = EditorGUILayout.TextField(new_key, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();
        #endregion

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(30));
        EditorGUILayout.LabelField("Key", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("Text", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();
        

        foreach (string key in phrases.Keys) {
            if (search != "" && key.IndexOf(search) < 0)
                continue;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                changed = true;
                phrases.Remove(key);
                EditorGUI.FocusTextInControl("");
                break;
            }

            EditorGUILayout.LabelField(key, GUILayout.Width(200));

            
            string text = EditorGUILayout.TextArea(phrases[key], GUI.skin.textArea, GUILayout.Width(300));
		

            EditorGUILayout.EndHorizontal();

            if (text != phrases[key]) {
                changed = true;
                phrases[key] = text;
                break;
            }
        }

        if (changed)
            Save();
    }

    public void Save() {
        Save(language.Value);
    }

    public static void Save(SystemLanguage language) {
        XmlDocument document = new XmlDocument();

        document.LoadXml("<language></language>");
        XmlElement root = document.DocumentElement;

        LocalizationAssistantEditor.content[language] = phrases;

        foreach (KeyValuePair<string, string> phrase in LocalizationAssistantEditor.content[language]) {
            XmlElement note = document.CreateElement("phrase");
            note.SetAttribute("key", phrase.Key);
            note.SetAttribute("text", phrase.Value);
            root.AppendChild(note);
        }

        string path = string.Format(LocalizationAssistant.filepath, language);
        document.Save(path);
    }

    public override UnityEngine.Object FindTarget() {
        if (LocalizationAssistant.main == null)
            LocalizationAssistant.main = FindObjectOfType<LocalizationAssistant>();
        return LocalizationAssistant.main;
    }
}

