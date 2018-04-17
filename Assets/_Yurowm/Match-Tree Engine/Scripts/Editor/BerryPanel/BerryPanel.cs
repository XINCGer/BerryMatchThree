using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Xml;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;
using EditorUtils;

public class BerryPanel : EditorWindow {

    const string helpLibraryLink = "http://" + "jellymobile.net/jellymobile.net/yurowm/helpLibrary.xml";
    Dictionary<string, Dictionary<string, string>> helpLibrary = new Dictionary<string, Dictionary<string, string>>();

    Texture BPicon;
    public string editorTitle = "";
    Color selectionColor;
    Color bgColor;

    List<Level> levels = new List<Level>();
    public static Level currentLevel;

    [MenuItem("Window/Berry Panel")]
    public static BerryPanel CreateBerryPanel() {
        BerryPanel window = GetWindow<BerryPanel>();
        window.titleContent = new GUIContent("Berry Panel");
        window.Show();
        window.OnEnable();
        return window;
    }

    void OnEnable() {
        BPicon = EditorGUIUtility.Load("BerryPanelIcon.png") as Texture;
        selectionColor = Color.Lerp(Color.red, Color.white, 0.7f);
        bgColor = Color.Lerp(GUI.backgroundColor, Color.black, 0.3f);
        showList = new AnimBool(false);
        showList.valueChanged.AddListener(Repaint);
        EditorCoroutine.start(DownloadHelpLibraryRoutine());
    }

    IEnumerator DownloadHelpLibraryRoutine() {
        WWW data = new WWW(helpLibraryLink);

        while (!data.isDone)
            yield return 0;

        if (!string.IsNullOrEmpty(data.error))
            yield break;

        XmlDocument xml = new XmlDocument();
        xml.LoadXml(data.text);

        helpLibrary.Clear();

        XmlNode root = xml.ChildNodes[0];

        foreach (XmlNode node in root.ChildNodes) {
            string _name = "";
            string _title = "";
            string _link = "";
            foreach (XmlAttribute attribute in node.Attributes) {
                if (attribute.Name == "title")
                    _title = attribute.Value;
                if (attribute.Name == "link")
                    _link = attribute.Value;
                if (attribute.Name == "name")
                    _name = attribute.Value;
            }
            if (_link == "" || _title == "" || _name == "")
                continue;

            if (!helpLibrary.ContainsKey(_title))
                helpLibrary.Add(_title, new Dictionary<string, string>());

            if (!helpLibrary[_title].ContainsKey(_name))
                helpLibrary[_title].Add(_name, _link);
        }
    }

    Color defalutColor;
    public Vector2 editorScroll, tabsScroll, levelScroll = new Vector2();
    public MetaEditor editor = null;

    public System.Action editorRender;
    void OnGUI() {
        if (BPicon != null)
            GUI.DrawTexture(EditorGUILayout.GetControlRect(GUILayout.Width(BPicon.width), GUILayout.Height(BPicon.height)), BPicon);

        if (editorRender == null || editor == null) {
            editorRender = null;
            editor = null;
        }

        defalutColor = GUI.backgroundColor;
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUI.backgroundColor = bgColor;
        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(150), GUILayout.ExpandHeight(true));
        GUI.backgroundColor = defalutColor;
        tabsScroll = EditorGUILayout.BeginScrollView(tabsScroll);

        DrawTabs();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        GUI.backgroundColor = bgColor;
        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUI.backgroundColor = defalutColor;
        editorScroll = EditorGUILayout.BeginScrollView(editorScroll);

        if (!string.IsNullOrEmpty(editorTitle))
            DrawTitle(editorTitle);


        if (editor != null)
            editorRender.Invoke();
        else
            GUILayout.Label("Nothing selected");

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("Berry Match Three, Fixer, Berry Panel\nYurov Viktor IE (Yurov Viktor) Copyright 2015 - 2016", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));
    }

    void DrawTabs() {
        if (DrawTabButton("Get PRO")) {
            editor = CreateInstance<PRORequired>();
            editorRender = editor.OnInspectorGUI;
        }

        DrawTabTitle("General");

        if (DrawTabButton("Content")) {
            editor = CreateInstance<ContentAssistantEditor>();
            editor.onRepaint += Repaint;
            editorRender = editor.OnInspectorGUI;
        }

        if (DrawTabButton("Localization")) {
            EditLocalization();
        }

        if (DrawTabButton("UI")) {
            editor = CreateInstance<UIAssistantEditor>();
            editorRender = editor.OnInspectorGUI;
        }

        if (DrawTabButton("Audio")) {
            editor = CreateInstance <AudioAssistantEditor>();
            editor.onRepaint += Repaint;
            editorRender = editor.OnInspectorGUI;
        }

        if (DrawTabButton("Spin Wheel Rewards")) {
            editor = CreateInstance<SpinWheelEditor>();
            editorRender = editor.OnInspectorGUI;
        }

        if (DrawTabButton("Combo Feedback")) {
            editor = CreateInstance<ComboFeedbackEditor>();
            editorRender = editor.OnInspectorGUI;
        }

        if (DrawTabButton("Local Profile")) {
            editor = CreateInstance<ProfileAssistantEditor>();
            editor.onRepaint += Repaint;
            editorRender = ((ProfileAssistantEditor) editor).DrawLocalProfile;
        }

        if (DrawTabButton("Bots")) {
            editor = CreateInstance<PRORequired>();
            editorRender = editor.OnInspectorGUI;
        }

        if (DrawTabButton("Local Notifications")) {
            editor = CreateInstance<PRORequired>();
            editorRender = editor.OnInspectorGUI;
        }

        if (DrawTabButton("Other")) {
            editor = CreateInstance<ProjectParametersEditor>();
            editorRender = editor.OnInspectorGUI;
        }

        DrawTabTitle("Levels");

        if (DrawTabButton("Level Editor")) {
            #region Level Editor
            levels = FindObjectsOfType<Level>().ToList();
            if (levels.Count > 0) {
                levels.Sort((Level a, Level b) => {
                    return a.transform.GetSiblingIndex() - b.transform.GetSiblingIndex();
                });
                UpdateLevelList();
                if (levels.Count > lastSelectedLevel) {
                    currentLevel = levels[lastSelectedLevel];
                } else
                    currentLevel = levels[0];
            }
            levelList = new ReorderableList(levels, typeof(Level), true, false, true, true);
            levelList.elementHeight = EditorGUIUtility.singleLineHeight + 6;

            levelList.drawElementCallback += DrawElement;

            levelList.onAddCallback += AddItem;
            levelList.onRemoveCallback += RemoveItem;
            levelList.onReorderCallback += ReorderItem;

            levelList.onSelectCallback += SelectItem;

            editorRender = LevelSelector;
            editor = CreateInstance<LevelEditor>();
            editorRender += editor.OnInspectorGUI;
            #endregion
        }

        if (DrawTabButton("Level Exporter")) {
            editor = CreateInstance<LevelExporter>();
            editor.onRepaint += Repaint;
            editorRender = editor.OnInspectorGUI;
        }

        DrawTabTitle("Monetization");

        if (DrawTabButton("Advertising")) {
            editor = CreateInstance<PRORequired>();
            editorRender = editor.OnInspectorGUI;
        }

        if (DrawTabButton("In-App Purchases")) {
            editor = CreateInstance<BerryStoreAssistantEditor>();
            editorRender = ((BerryStoreAssistantEditor) editor).DrawIAPs;
        }

        if (DrawTabButton("Item IDs")) {
            editor = CreateInstance<BerryStoreAssistantEditor>();
            editorRender = ((BerryStoreAssistantEditor) editor).DrawItems;
        }

        if (DrawTabButton("Initial Inventory")) {
            editor = CreateInstance<ProfileAssistantEditor>();
            editorRender = ((ProfileAssistantEditor) editor).DrawInitialInventory;
        }

        DrawTabTitle("Items");

        if (DrawTabButton("Chip Settings")) {
            editor = CreateInstance<SessionAssistantEditor>();
            editorRender = ((SessionAssistantEditor) editor).DrawChips;
        }
        if (DrawTabButton("Chip Combinations")) {
            editor = CreateInstance<SessionAssistantEditor>();
            editorRender = ((SessionAssistantEditor) editor).DrawCombinations;
        }
        if (DrawTabButton("Chip Mixes")) {
            editor = CreateInstance<SessionAssistantEditor>();
            editorRender = ((SessionAssistantEditor) editor).DrawMixes;
        }
        if (DrawTabButton("Blockers")) {
            editor = CreateInstance<SessionAssistantEditor>();
            editorRender = ((SessionAssistantEditor) editor).DrawBlockers;
        }

        DrawTabTitle("Development");

        if (DrawTabButton("Fixer")) {
            editor = CreateInstance<Berry.Fixer.Fixer>();
            editor.onRepaint += Repaint;
            editorRender = editor.OnInspectorGUI;
        }
        if (DrawTabButton("Contact")) {
            editor = CreateInstance<ContactForm>();
            editor.onRepaint += Repaint;
            editorRender = editor.OnInspectorGUI;
        }
    }

    bool DrawTabButton(string text) {
        Color color = GUI.backgroundColor;
        if (editorTitle == text)
            GUI.backgroundColor = selectionColor;
        bool result = GUILayout.Button(text, EditorStyles.miniButton, GUILayout.ExpandWidth(true));
        GUI.backgroundColor = color;

        if (string.IsNullOrEmpty(editorTitle) || (editorTitle == text && editorRender == null))
            result = true;

        if (result) {
            EditorGUI.FocusTextInControl("");
            editorTitle = text;
        }

        return result;
    }

    void DrawTabTitle(string text) {
        GUILayout.Label(text, EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));
    }

    void DrawTitle(string text) {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(text, EditorStyles.largeLabel, GUILayout.ExpandWidth(true));

        if (helpLibrary.ContainsKey(text)) {
            foreach (string key in helpLibrary[text].Keys) {
                GUIContent content = new GUIContent(key);
                if (GUILayout.Button(key, EditorStyles.miniButton, GUILayout.Width(EditorStyles.miniButton.CalcSize(content).x)))
                    Application.OpenURL(helpLibrary[text][key]);
            }
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
    }


    public void EditLocalization(string _search = "") {
        if (!LocalizationAssistant.main)
            LocalizationAssistant.main = FindObjectOfType<LocalizationAssistant>();
        if (LocalizationAssistant.main && LocalizationAssistant.main.languages.Count > 0)
            EditLocalization(LocalizationAssistant.main.languages[0], _search);
    }

    public void EditLocalization(SystemLanguage _language, string _search = "") {
        editor = new LocalizationAssistantEditor();
        editorRender = editor.OnInspectorGUI;
        //LocalizationEditor locEditor = new LocalizationEditor();
        LocalizationEditor locEditor = CreateInstance<LocalizationEditor>(); // FIXPACK
        locEditor.scroll = (Vector2 scroll) => {
            editorScroll = scroll;
        };
        editorRender += locEditor.OnInspectorGUI;
        locEditor.language = _language;
        locEditor.search = _search;

        locEditor.OnEnable();

        editorTitle = "Localization";
    }

    #region Level Editor
    public static int lastSelectedLevel {
        get {
            return PlayerPrefs.GetInt("Editor_lastSelectedLevel");
        }
        set {
            PlayerPrefs.SetInt("Editor_lastSelectedLevel", value);
        }
    }
    ReorderableList levelList;
    AnimBool showList = new AnimBool(false);
    void LevelSelector() {
        showList.target = GUILayout.Toggle(showList.target, "Level List", EditorStyles.foldout);
        if (EditorGUILayout.BeginFadeGroup(showList.faded))
            levelList.DoLayoutList();
        EditorGUILayout.EndFadeGroup();
    }

    void DrawElement(Rect rect, int index, bool isActive, bool isFocused) {
        if (index != Mathf.Clamp(index, 0, levels.Count - 1))
            return;

        Color defColor = GUI.backgroundColor;
        if (index % 2 == 0)
            GUI.backgroundColor = Color.Lerp(defColor, Color.red, 0.2f);
        GUI.Box(rect, "", EditorStyles.textArea);
        GUI.backgroundColor = defColor;

        Level level = levels[index];

        Rect _rect = rect;
        _rect.width = 30;
        _rect.height = rect.height;
        GUI.Label(_rect, (index + 1).ToString() + ".", EditorStyles.boldLabel);

        _rect.x += _rect.width;
        _rect.width = rect.width - _rect.width;
        GUI.Label(_rect, "Target: " + level.profile.target + " Limitation: " + level.profile.limitation);
    }

    void AddItem(ReorderableList list) {
        Transform parent = null;
        if (levels.Count > 0)
            parent = levels[0].transform.parent;
        else {
            GameObject obj = GameObject.Find("Levels");
            if (obj && obj.transform.parent == null)
                parent = obj.transform;
            else
                parent = new GameObject("Levels").transform;
        }

        GameObject level_object = new GameObject();
        level_object.transform.SetParent(parent);
        Level level = level_object.AddComponent<Level>();
        level.profile = new LevelProfile();

        list.list.Add(level);

        levels = new List<Level>(list.list.Cast<Level>());

        if (levels.Count > 0) {
            UpdateLevelList();
            list.index = levels.Count - 1;
            currentLevel = levels[list.index];
        }

    }

    void RemoveItem(ReorderableList list) {
        int id = list.index;
        list.list.RemoveAt(id);
        DestroyImmediate(levels[id].gameObject);

        levels = new List<Level>(list.list.Cast<Level>());

        if (levels.Count > 0) {
            UpdateLevelList();
            id = Mathf.Clamp(id, 0, levels.Count - 1);
            currentLevel = levels[id];
            levelList.index = id;
        }


    }

    void ReorderItem(ReorderableList list) {
        levels = new List<Level>(list.list.Cast<Level>());
        for (int i = 0; i < levels.Count; i++) {
            levels[i].transform.SetSiblingIndex(i);
            levels[i].profile.level = i + 1;
            LevelEditor.UpdateName(levels[i]);
        }

    }

    void SelectItem(ReorderableList list) {
        currentLevel = levels[list.index];
        lastSelectedLevel = list.index;
        UpdateLevelList();
    }

    void UpdateLevelList() {
        foreach (Level l in levels) {
            l.profile.level = l.transform.GetSiblingIndex() + 1;
            LevelEditor.UpdateName(l);
        }
    }
    #endregion
}