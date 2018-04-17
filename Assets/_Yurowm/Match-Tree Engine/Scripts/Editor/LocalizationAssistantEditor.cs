using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditorUtils;

[CustomEditor(typeof(LocalizationAssistant))]
public class LocalizationAssistantEditor : MetaEditor {

    public static Dictionary<SystemLanguage, Dictionary<string, string>> content;
    public static List<string> keys = new List<string>();
    public static string[] keys_menu;

    LocalizationAssistant main;

    SystemLanguage new_language;
    public override void OnInspectorGUI() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("LocalizationAssistant is missing", MessageType.Error);
            return;
        }
        main = (LocalizationAssistant) metaTarget;
        Undo.RecordObject(main, "localization changings");

        if (main.languages.Count == 0)
            main.languages.Add(SystemLanguage.English);

        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.ExpandWidth(true));
        main.use_system_language_by_default = GUILayout.Toggle(main.use_system_language_by_default, "Use system language by default");
        if (!main.use_system_language_by_default) {
            List<string> languages = main.languages.Select(x => x.ToString()).ToList();
            int id = languages.IndexOf(main.default_language.ToString());
            if (id < 0) id = 0;
            id = EditorGUILayout.Popup("Default language", id, languages.ToArray());
            main.default_language = (SystemLanguage) System.Enum.Parse(typeof(SystemLanguage), languages[id]);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.ExpandWidth(true));

        #region Languages list panel
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("-", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(20));
        GUILayout.Label("Edit", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(35));
        GUILayout.Label("Name", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(200));

        EditorGUILayout.EndHorizontal();

        foreach (SystemLanguage language in main.languages) {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                main.languages.Remove(language);
                break;
            }
            if (GUILayout.Button("Edit", GUILayout.Width(35))) {
                BerryPanel.CreateBerryPanel().EditLocalization(language);
            }
            EditorGUILayout.LabelField(language.ToString(), GUILayout.Width(200));

            EditorGUILayout.EndHorizontal();
        }
        #endregion

        EditorGUILayout.Space();


        #region "Add new" panel
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = !main.languages.Contains(new_language);
        if (GUILayout.Button("Add", GUILayout.Width(40)))
            main.languages.Add(new_language);
        GUI.enabled = true;

        new_language = (SystemLanguage) EditorGUILayout.EnumPopup(new_language, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        #endregion

        EditorGUILayout.EndVertical();
    }


    public static void LoadContent() {
        LocalizationAssistant main = FindObjectOfType<LocalizationAssistant>();
        content = null;
        if (!main) return;

        content = new Dictionary<SystemLanguage, Dictionary<string, string>>();
        foreach (SystemLanguage language in main.languages)
            content.Add(language, LocalizationAssistant.Load(language));

        keys.Clear();
        foreach (Dictionary<string, string> _phrases in content.Values)
            keys.AddRange(_phrases.Keys);
        keys = keys.Distinct().ToList();
        keys.Sort();
        UpdateKeysMenu();
    }

    public static void UpdateKeysMenu() {
        keys_menu = keys.Select(x => ((string) x.Clone()).Replace('_', '/')).ToArray();
    }

    public override Object FindTarget() {
        if (LocalizationAssistant.main == null)
            LocalizationAssistant.main = FindObjectOfType<LocalizationAssistant>();
        return LocalizationAssistant.main;
    }
}
