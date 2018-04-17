using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;

public class LocalizationAssistant : MonoBehaviour {

    public static LocalizationAssistant main;
    public static readonly string filepath = "Assets/Resources/Languages/{0}.xml";

    public List<SystemLanguage> languages = new List<SystemLanguage>();
    public SystemLanguage current_language;
    public SystemLanguage default_language;
    public bool use_system_language_by_default = true;

    public Dictionary<string, string> phrases = new Dictionary<string, string>();

    void Awake () {
        main = this;
        if (PlayerPrefs.HasKey("Language")) {
            current_language = (SystemLanguage) Enum.Parse(typeof(SystemLanguage), PlayerPrefs.GetString("Language"));
        } else {
            current_language = use_system_language_by_default ? Application.systemLanguage : default_language;
            PlayerPrefs.SetString("Language", current_language.ToString());
        }
        if (!languages.Contains(current_language))
            current_language = SystemLanguage.English;
        if (!languages.Contains(current_language))
            current_language = languages[0];

        LearnLanguage(current_language);
    }

    public void LearnLanguage(SystemLanguage language) {
        DebugPanel.Log("Language", "System", language);
        current_language = language;
        PlayerPrefs.SetString("Language", current_language.ToString());
        phrases = Load(current_language);
    }

    public string this[string index] {
        get {
            if (!phrases.ContainsKey(index))
                phrases.Add(index, index);
            return phrases[index];
        }
        set {
            if (!phrases.ContainsKey(index))
                phrases.Add(index, value);
            else
                phrases[index] = value;
        }
    }

    public static Dictionary<string, string> Load(SystemLanguage language) {
        XmlDocument document = new XmlDocument();
        Dictionary<string, string> result = new Dictionary<string, string>();

        if (Application.isEditor) {
            if (!(new FileInfo(string.Format(filepath, language)).Exists))
                return result;
            document.Load(string.Format(filepath, language));
        } else {
            TextAsset text = Resources.Load("Languages/" + language.ToString()) as TextAsset;
            if (text == null || string.IsNullOrEmpty(text.text)) 
                return result;
            document.LoadXml(text.text);
        }
        XmlElement root = document.DocumentElement;

        foreach (XmlNode node in root.ChildNodes)
            if (node.Name == "phrase")
                result[node.Attributes[0].Value] = node.Attributes[1].Value;

        return result;
    }
}

public interface INeedLocalization {
    List<string> RequriedLocalizationKeys();
}