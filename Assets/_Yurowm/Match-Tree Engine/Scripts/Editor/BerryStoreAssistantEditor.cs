using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using EditorUtils;

[CustomEditor(typeof(BerryStoreAssistant))]
public class BerryStoreAssistantEditor : MetaEditor {

    private BerryStoreAssistant main;


    AnimBool iapsFade = new AnimBool(false);
    AnimBool itemsFade = new AnimBool(false);

    public override void OnInspectorGUI() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("BerryStoreAssistant is missing", MessageType.Error);
            return;
        }
        main = (BerryStoreAssistant) metaTarget;

        if (main.items == null)
            main.items = new List<BerryStoreAssistant.ItemInfo>();

        if (main.iaps == null)
            main.iaps = new List<BerryStoreAssistant.IAP>();
        
        #region Items
        itemsFade.target = GUILayout.Toggle(itemsFade.target, "Items", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(itemsFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            DrawItems();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        #region IAPs
        iapsFade.target = GUILayout.Toggle(iapsFade.target, "IAPs", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(iapsFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            DrawIAPs();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion
    }

    public void DrawIAPs() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("BerryStoreAssistant is missing", MessageType.Error);
            return;
        }
        main = (BerryStoreAssistant) metaTarget;
        Undo.RecordObject(main, "");

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("ID", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
        GUILayout.Label("SKU", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();

        foreach (BerryStoreAssistant.IAP iap in main.iaps) {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                main.iaps.Remove(iap);
                break;
            }

            iap.id = EditorGUILayout.TextField(iap.id, GUILayout.Width(100));
            iap.sku = EditorGUILayout.TextField(iap.sku, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add", GUILayout.Width(60)))
            main.iaps.Add(new BerryStoreAssistant.IAP());

        EditorGUILayout.EndHorizontal();
    }

    public  void DrawItems() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("BerryStoreAssistant is missing", MessageType.Error);
            return;
        }
        main = (BerryStoreAssistant) metaTarget;
        Undo.RecordObject(main, "");

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("Name", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(80));
        GUILayout.Label("ID", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60));
        GUILayout.Label("Localiz. Name", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));
        GUILayout.Label("Localiz. Descipt.", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));

        EditorGUILayout.EndHorizontal();

        foreach (BerryStoreAssistant.ItemInfo item in main.items) {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                main.items.Remove(item);
                break;
            }
            item.name = EditorGUILayout.TextField(item.name, GUILayout.Width(80));
            item.id = EditorGUILayout.TextField(item.id, GUILayout.Width(60));
            EditorGUILayout.LabelField(item.localization_name, GUILayout.Width(150));
            EditorGUILayout.LabelField(item.localization_description, GUILayout.Width(150));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add", GUILayout.Width(60)))
            main.items.Add(new BerryStoreAssistant.ItemInfo());
        if (GUILayout.Button("Edit Localization", GUILayout.Width(150))) {
            BerryPanel.CreateBerryPanel().EditLocalization("item_");
        }

        EditorGUILayout.EndHorizontal();
    }

    public override Object FindTarget() {
        if (BerryStoreAssistant.main == null)
            BerryStoreAssistant.main = FindObjectOfType<BerryStoreAssistant>();
        return BerryStoreAssistant.main;
    }

    public BerryStoreAssistantEditor () {
        itemsFade.valueChanged.AddListener(RepaintIt);
        iapsFade.valueChanged.AddListener(RepaintIt);
    }
}
