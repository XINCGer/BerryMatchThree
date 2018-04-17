using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;
using EditorUtils;

[CustomEditor(typeof(SessionAssistant))]
public class SessionAssistantEditor : MetaEditor {

    SessionAssistant main;

    AnimBool combinationsFade = new AnimBool(false);
    AnimBool chipsFade = new AnimBool(false);
    AnimBool mixesFade = new AnimBool(false);
    AnimBool generalFade = new AnimBool(false);
    AnimBool blockersFade = new AnimBool();
    AnimBool spinFade = new AnimBool(false);
    AnimBool notificationsFade = new AnimBool(false);

    Color defaultColor;

    public override void OnInspectorGUI() {
        if (metaTarget == null) return;
        main = (SessionAssistant) metaTarget;
        defaultColor = GUI.color;

        if (main.combinations == null)
            main.combinations = new List<SessionAssistant.Combinations>();

        #region Chips
        chipsFade.target = GUILayout.Toggle(chipsFade.target, "Chips", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(chipsFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            DrawChips();
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        #region Blockers
        blockersFade.target = GUILayout.Toggle(blockersFade.target, "Blockers", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(blockersFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            DrawBlockers();
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        #region Combinations
        combinationsFade.target = GUILayout.Toggle(combinationsFade.target, "Combinations", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(combinationsFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            DrawCombinations();
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        #region Mixes
        mixesFade.target = GUILayout.Toggle(mixesFade.target, "Mixes", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(mixesFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            DrawMixes();
           
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        GUI.color = defaultColor;
    }

    public void DrawMixes() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("SessionAssistant is missing", MessageType.Error);
            return;
        }
        main = (SessionAssistant) metaTarget;
        Undo.RecordObject(main, "");

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("First type", GUILayout.Width(100));
        GUILayout.Label("Second type", GUILayout.Width(100));
        GUILayout.Label("Target type", GUILayout.Width(100));
        GUILayout.Label("Message", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        List<string> chips = new List<string>();
        foreach (SessionAssistant.ChipInfo info in main.chipInfos)
            if (!chips.Contains(info.name))
                chips.Add(info.name);

        foreach (SessionAssistant.Mix mix in main.mixes) {
            EditorGUILayout.BeginHorizontal();


            if (chips.Count > 0) {
                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    main.mixes.Remove(mix);
                    break;
                }

                int id = chips.IndexOf(mix.pair.a);
                if (id == -1)
                    id = 0;
                id = EditorGUILayout.Popup(id, chips.ToArray(), GUILayout.Width(80));
                mix.pair.a = chips[id];

                id = chips.IndexOf(mix.pair.b);
                if (id == -1)
                    id = 0;
                id = EditorGUILayout.Popup(id, chips.ToArray(), GUILayout.Width(80));
                mix.pair.b = chips[id];

                GUILayout.Label((string) mix.pair.a, EditorStyles.boldLabel, GUILayout.Width(100));

                mix.function = EditorGUILayout.TextField(mix.function, GUILayout.Width(100));
            }


            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add", GUILayout.Width(60)))
            main.mixes.Add(new SessionAssistant.Mix());

        EditorGUILayout.EndHorizontal();
    }

    public void DrawCombinations() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("SessionAssistant is missing", MessageType.Error);
            return;
        }
        main = (SessionAssistant) metaTarget;
        Undo.RecordObject(main, "");

        if (main.chipInfos.Count == 0)
            EditorGUILayout.HelpBox("No powerups found", MessageType.Error, true);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("Priority", GUILayout.Width(80));
        if (main.squareCombination)
            GUILayout.Label("Square", GUILayout.Width(50));
        GUILayout.Label("Vert.", GUILayout.Width(40));
        GUILayout.Label("Horiz.", GUILayout.Width(40));
        GUILayout.Label("Count", GUILayout.Width(40));
        GUILayout.Label("PowerUp", GUILayout.Width(80));

        EditorGUILayout.EndHorizontal();

        List<string> chips = new List<string>();
        foreach (SessionAssistant.ChipInfo info in main.chipInfos)
            if (!chips.Contains(info.name))
                chips.Add(info.name);

        foreach (SessionAssistant.Combinations combination in main.combinations) {
            if (!main.squareCombination && combination.square)
                continue;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                main.combinations.Remove(combination);
                break;
            }
            combination.priority = EditorGUILayout.IntField(combination.priority, GUILayout.Width(80));
            if (main.squareCombination) {
                combination.square = EditorGUILayout.Toggle(combination.square, GUILayout.Width(50));
                GUI.enabled = !combination.square;
            }
            combination.vertical = EditorGUILayout.Toggle(combination.vertical, GUILayout.Width(40));
            combination.horizontal = EditorGUILayout.Toggle(combination.horizontal, GUILayout.Width(40));
            combination.minCount = Mathf.Clamp(EditorGUILayout.IntField(combination.minCount, GUILayout.Width(40)), 4, 9);
            GUI.enabled = true;

            if (chips.Count > 0) {
                int id = chips.IndexOf(combination.chip);
                if (id == -1)
                    id = 0;
                id = EditorGUILayout.Popup(id, chips.ToArray(), GUILayout.Width(80));
                combination.chip = chips[id];
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add", GUILayout.Width(60)))
            main.combinations.Add(new SessionAssistant.Combinations());
        if (GUILayout.Button("Sort", GUILayout.Width(60)))
            main.combinations.Sort((SessionAssistant.Combinations a, SessionAssistant.Combinations b) => {
                if (a.priority < b.priority)
                    return -1;
                if (a.priority > b.priority)
                    return 1;
                return 0;
            });

        EditorGUILayout.EndHorizontal();
    }

    public void DrawBlockers() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("SessionAssistant is missing", MessageType.Error);
            return;
        }
        main = (SessionAssistant) metaTarget;
        Undo.RecordObject(main, "");

        if (!ContentAssistant.main) {
            ContentAssistant.main = GameObject.FindObjectOfType<ContentAssistant>();
            ContentAssistant.main.Initialize();
        }

        if (!ContentAssistant.main) {
            EditorGUILayout.HelpBox("ContentAssistant is missing", MessageType.Error);
            return;
        }

        defaultColor = GUI.color;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("Name", GUILayout.Width(100));
        GUILayout.Label("Content Name", GUILayout.Width(100));
        GUILayout.Label("Levels", GUILayout.Width(50));
        GUILayout.Label("Chip", GUILayout.Width(40));
        GUILayout.Label("LE_Name", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        foreach (SessionAssistant.BlockInfo blockInfo in main.blockInfos) {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                main.blockInfos.Remove(blockInfo);
                break;
            }

            blockInfo.name = EditorGUILayout.TextField(blockInfo.name, GUILayout.Width(100));
            blockInfo.contentName = EditorGUILayout.TextField(blockInfo.contentName, GUILayout.Width(100));
            if (ContentAssistant.main) {
                IBlock iblock = ContentAssistant.main.GetPrefab<IBlock>(blockInfo.contentName);
                if (iblock) {
                    blockInfo.levelCount = iblock.GetLevels();
                    blockInfo.chip = iblock.CanItContainChip();
                }
            }
            GUILayout.Label(blockInfo.levelCount > 0 ? blockInfo.levelCount.ToString() : "N/A", GUILayout.Width(50));
            GUILayout.Toggle(blockInfo.chip, "", GUILayout.Width(40));
            blockInfo.shirtName = EditorGUILayout.TextField(blockInfo.shirtName, GUILayout.Width(30));
            if (blockInfo.shirtName.Length > 2)
                blockInfo.shirtName = blockInfo.shirtName.Substring(0, 2);


            EditorGUILayout.EndHorizontal();

            GUI.color = Color.red;
            if (ContentAssistant.main) {
                if (!ContentAssistant.main.cItems.Exists(x => x.item.name == blockInfo.contentName))
                    EditorGUILayout.LabelField("'" + blockInfo.contentName + "' is missing", EditorStyles.boldLabel, GUILayout.Width(250));

            }
            GUI.color = defaultColor;
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add", GUILayout.Width(60)))
            main.blockInfos.Add(new SessionAssistant.BlockInfo());

        EditorGUILayout.EndHorizontal();
    }

    public void DrawChips() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("SessionAssistant is missing", MessageType.Error);
            return;
        }
        main = (SessionAssistant) metaTarget;
        Undo.RecordObject(main, "");

        if (!ContentAssistant.main)
            ContentAssistant.main = GameObject.FindObjectOfType<ContentAssistant>();

        if (!ContentAssistant.main) {
            EditorGUILayout.HelpBox("ContentAssistant is missing", MessageType.Error);
            return;
        }


        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("Name", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(120));
        GUILayout.Label("Content Name", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(120));
        GUILayout.Label("Color", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(40));
        GUILayout.Label("LE_Name", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        defaultColor = GUI.color;

        if (main.chipInfos.Count == 0 || main.chipInfos[0].name != "SimpleChip") {
            main.chipInfos.Insert(0, new SessionAssistant.ChipInfo());
            main.chipInfos[0].name = "SimpleChip";
            main.chipInfos[0].contentName = "SimpleChip";
            main.chipInfos[0].color = true;
            main.chipInfos[0].shirtName = "";
        }

        foreach (SessionAssistant.ChipInfo chipInfo in main.chipInfos) {
            EditorGUILayout.BeginHorizontal();

            if (chipInfo == main.chipInfos[0])
                GUI.enabled = false;

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                main.chipInfos.Remove(chipInfo);
                break;
            }
            chipInfo.name = EditorGUILayout.TextField(chipInfo.name, GUILayout.Width(120));
            chipInfo.contentName = EditorGUILayout.TextField(chipInfo.contentName, GUILayout.Width(120));
            chipInfo.color = EditorGUILayout.Toggle(chipInfo.color, GUILayout.Width(40));
            chipInfo.shirtName = EditorGUILayout.TextField(chipInfo.shirtName, GUILayout.Width(30));
            if (chipInfo.shirtName.Length > 2)
                chipInfo.shirtName = chipInfo.shirtName.Substring(0, 2);


            EditorGUILayout.EndHorizontal();

            GUI.color = Color.red;
            if (ContentAssistant.main) {
                if (chipInfo.color) {
                    foreach (string color in Chip.chipTypes) {
                        if (!ContentAssistant.main.cItems.Exists(x => x.item.name == chipInfo.contentName + color))
                            EditorGUILayout.LabelField("'" + chipInfo.contentName + color + "' is missing", EditorStyles.boldLabel, GUILayout.Width(250));
                    }
                } else
                    if (!ContentAssistant.main.cItems.Exists(x => x.item.name == chipInfo.contentName))
                    EditorGUILayout.LabelField("'" + chipInfo.contentName + "' is missing", EditorStyles.boldLabel, GUILayout.Width(250));

            }
            GUI.color = defaultColor;
            GUI.enabled = true;
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add", GUILayout.Width(60)))
            main.chipInfos.Add(new SessionAssistant.ChipInfo());

        EditorGUILayout.EndHorizontal();
    }

    void OnEnable () {
        chipsFade.valueChanged.AddListener(RepaintIt);
        blockersFade.valueChanged.AddListener(RepaintIt);
        combinationsFade.valueChanged.AddListener(RepaintIt);
        mixesFade.valueChanged.AddListener(RepaintIt);
        generalFade.valueChanged.AddListener(RepaintIt);
        spinFade.valueChanged.AddListener(RepaintIt);
        notificationsFade.valueChanged.AddListener(RepaintIt);
    }

    public override Object FindTarget() {
        if (SessionAssistant.main == null)
            SessionAssistant.main = FindObjectOfType<SessionAssistant>();
        return SessionAssistant.main;
    }
}
