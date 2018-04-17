using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using EditorUtils;
using System.Linq;

[CustomEditor(typeof(ProfileAssistant))]
public class ProfileAssistantEditor : MetaEditor {

    ProfileAssistant main;
    AnimBool inventoryFade = new AnimBool(false);
    AnimBool scoresFade = new AnimBool(false);
    AnimBool localProfileFade = new AnimBool(false);
    AnimBool botsFade = new AnimBool(false);
    AnimBool initialInventoryFade = new AnimBool(false);

    void OnEnable() {
        scoresFade.valueChanged.AddListener(RepaintIt);
        inventoryFade.valueChanged.AddListener(RepaintIt);
        botsFade.valueChanged.AddListener(RepaintIt);
        localProfileFade.valueChanged.AddListener(RepaintIt);
        botsFade.valueChanged.AddListener(RepaintIt);
        initialInventoryFade.valueChanged.AddListener(RepaintIt);
    }

    public override void OnInspectorGUI() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("ProfileAssistant is missing", MessageType.Error);
            return;
        }
        main = (ProfileAssistant) metaTarget;
        Undo.RecordObject(main, "");

        #region Local Profile
        localProfileFade.target = GUILayout.Toggle(localProfileFade.target, "Local Profile", EditorStyles.foldout);
        if (EditorGUILayout.BeginFadeGroup(localProfileFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            DrawLocalProfile();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        #region Initial Inventory
        initialInventoryFade.target = GUILayout.Toggle(initialInventoryFade.target, "Initial Inventory", EditorStyles.foldout);
        if (EditorGUILayout.BeginFadeGroup(initialInventoryFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            DrawInitialInventory();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion
    }

    public void DrawInitialInventory() {
        main = (ProfileAssistant) metaTarget;
        Undo.RecordObject(main, "");

        if (BerryStoreAssistant.main == null)
            BerryStoreAssistant.main = FindObjectOfType<BerryStoreAssistant>();

        if (BerryStoreAssistant.main == null) {
            EditorGUILayout.HelpBox("BerryStoreAssistant is missing", MessageType.Error);
            return;
        }

        
        if (ProjectParameters.main == null)
            ProjectParameters.main = FindObjectOfType<ProjectParameters>();

        if (ProjectParameters.main == null) {
            EditorGUILayout.HelpBox("ProjectParameters is missing", MessageType.Error);
            return;
        }
		#region Header
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Item ID", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(120));
        GUILayout.Label("Count", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();
        #endregion


        List<string> items = BerryStoreAssistant.main.items.Select(x => x.id).ToList();
        Dictionary<string, int> inventory = new Dictionary<string, int>();
        if (!string.IsNullOrEmpty(main.firstStartInventory))
            inventory = main.firstStartInventory.Split(';').Select(x => x.Split(':')).ToDictionary(x => x[0], x => int.Parse(x[1]));

        string result = "";

        bool isLife;
        foreach (string item in items) {
            isLife = item == "life";

            GUI.enabled = !isLife;
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(item, GUILayout.Width(120));
            int count = inventory.ContainsKey(item) ? inventory[item] : 0;
            if (isLife) count = ProjectParameters.main.lifes_limit;
            count = Mathf.Max(0, EditorGUILayout.IntField(count, GUILayout.Width(120)));
            if (result != "") result += ";";
            result += item + ":" + count;

            EditorGUILayout.EndHorizontal();
        }

        main.firstStartInventory = result;
    }

    public void DrawLocalProfile() {
        main = (ProfileAssistant) metaTarget;
        Undo.RecordObject(main, "");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Data", GUILayout.Width(80))) {
            main.ClearData();
        }
        if (GUILayout.Button("Unlock All Levels", GUILayout.Width(110))) {
            main.UnlockAllLevels();
        }
        EditorGUILayout.EndHorizontal();

        if (main.local_profile == null)
            main.local_profile = UserProfileUtils.ReadProfileFromDevice();

        EditorGUILayout.LabelField("Name", main.local_profile.name.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Facebook ID", main.local_profile.facebookID.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Current level", main.local_profile.current_level.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Last save", main.local_profile.lastSave.ToShortDateString() + " " + main.local_profile.lastSave.ToLongTimeString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Next Life time", main.local_profile.next_life_time.ToShortDateString() + " " + main.local_profile.next_life_time.ToLongTimeString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Daily Reward", main.local_profile.daily_raward.ToShortDateString() + " " + main.local_profile.daily_raward.ToLongTimeString(), EditorStyles.boldLabel);

        inventoryFade.target = GUILayout.Toggle(inventoryFade.target, "Inventory", EditorStyles.foldout);
        if (EditorGUILayout.BeginFadeGroup(inventoryFade.faded)) {
            if (main.local_profile.inventory.Count > 0) {
                foreach (KeyValuePair<string, int> inventory in main.local_profile.inventory) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField(inventory.Key, inventory.Value.ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                }
            } else
                GUILayout.Label("Empty");
        }
        EditorGUILayout.EndFadeGroup();

        scoresFade.target = GUILayout.Toggle(scoresFade.target, "Score", EditorStyles.foldout);
        if (EditorGUILayout.BeginFadeGroup(scoresFade.faded)) {
            if (main.local_profile.score.Count > 0) {
                foreach (KeyValuePair<int, int> score in main.local_profile.score) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("Level " + score.Key.ToString(), score.Value.ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                }
            } else
                GUILayout.Label("Empty");
        }
        EditorGUILayout.EndFadeGroup();
    }

    public override Object FindTarget() {
        if (ProfileAssistant.main == null)
            ProfileAssistant.main = FindObjectOfType<ProfileAssistant>();
        return ProfileAssistant.main;
    }
}

