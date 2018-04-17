using UnityEngine;
//using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(Chip))]
[CanEditMultipleObjects]
public class ChipEditor : Editor {

    Chip chip;
    SerializedProperty chip_id;
    SerializedProperty chip_type;
    IChipLogic logic;

	void OnEnable () {
        chip = (Chip) target;
        chip_id = serializedObject.FindProperty("id");

        logic = chip.GetComponent<IChipLogic>();
        if (logic != null) {
            chip.chipType = logic.GetChipType();
            chip_type = serializedObject.FindProperty("chipType");
        }
        IAnimateChip[] components = chip.GetComponents<IAnimateChip>();
        List<Clip> clips = new List<Clip>();
        foreach (IAnimateChip component in components)
            clips.AddRange(component.GetClipNames().Select(x => new Clip(x)).ToList());
        foreach (Clip clip in clips)
            if (!chip.clips_serialized.Contains(clip))
                chip.clips_serialized.Add(clip);
        foreach (Clip clip in new List<Clip>(chip.clips_serialized))
            if (!clips.Contains(clip) && chip.clips_serialized.Contains(clip))
                chip.clips_serialized.Remove(clip);

        chip.clips_serialized.Sort((Clip a, Clip b) => {
            return string.CompareOrdinal(a.name, b.name);
        });
    }


    public override void OnInspectorGUI() {
        Undo.RecordObjects(serializedObject.targetObjects, "Chip changed");
        serializedObject.Update();
        Color defaultColor = GUI.backgroundColor;
        Color color;
        if (!chip_id.hasMultipleDifferentValues && chip_id.intValue == Mathf.Clamp(chip_id.intValue, 0, Chip.colors.Length - 1))
            color = Chip.colors[chip.id];
        else
            color = Color.gray;

        #region Parameters
        GUI.backgroundColor = color;
        EditorGUILayout.BeginVertical(EditorStyles.miniButton);
        GUI.backgroundColor = defaultColor;
        EditorGUILayout.LabelField("Parameters", EditorStyles.largeLabel);

        if (serializedObject.isEditingMultipleObjects) {
            if (chip_type.hasMultipleDifferentValues)
                EditorGUILayout.LabelField("Type", "-");
            else
                EditorGUILayout.LabelField("Type", logic.GetChipType());
        } else {
            if (logic != null)
                EditorGUILayout.LabelField("Type", logic.GetChipType());
            else
                EditorGUILayout.HelpBox("IChipLogic component is missing", MessageType.Error);
        }

        List<string> colors = new List<string>(Chip.chipTypes);
        List<int> indexes = new List<int>();
        for (int i = 0; i < colors.Count; i++)
            indexes.Add(i);
        colors.Add("Universal");
        indexes.Add(Chip.universalColorId);
        colors.Add("Uncolored");
        indexes.Add(Chip.uncoloredId);

        EditorGUILayout.IntPopup(chip_id, colors.ToArray().Select(x => new GUIContent(x)).ToArray(), indexes.ToArray(), new GUIContent("Color ID"));

        EditorGUILayout.EndVertical();
        #endregion

        GUILayout.Space(10);

        #region Animations     
        GUI.backgroundColor = color;
        EditorGUILayout.BeginVertical(EditorStyles.miniButton);
        GUI.backgroundColor = defaultColor;
        EditorGUILayout.LabelField("Animations", EditorStyles.largeLabel);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.Label("Clip", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        
        List<string> clips = new List<string>();
        if (serializedObject.isEditingMultipleObjects) {
            List<IAnimateChip> components = new List<IAnimateChip>();
            foreach (Chip obj in serializedObject.targetObjects)
                components.AddRange(obj.GetComponents<IAnimateChip>());
            foreach (IAnimateChip component in components)
                clips.AddRange(component.GetClipNames());
            clips = clips.Distinct().ToList();
            clips.Sort();
        } else
            clips = chip.clips_serialized.Select(x => x.name).ToList();

        Dictionary<string, bool> multi = new Dictionary<string, bool>();
        if (serializedObject.isEditingMultipleObjects) {
            foreach (string clip_name in clips) {
                multi.Add(clip_name, false);
                AnimationClip value = null;
                foreach (Chip obj in serializedObject.targetObjects) {
                    int i = obj.clips_serialized.FindIndex(x => x.name == clip_name);
                    if (i < 0)
                        continue;
                    if (value == null) {
                        value = obj.clips_serialized[i].clip;
                        continue;
                    }
                    if (value != obj.clips_serialized[i].clip) {
                        multi[clip_name] = true;
                        break;
                    }
                }
            }
        } else
            multi = clips.ToDictionary(x => x, x => false);


        AnimationClip temp = new AnimationClip();
        foreach (string clip_name in clips) {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(clip_name, GUILayout.Width(100));


            if (multi[clip_name]) {
                EditorGUI.showMixedValue = true;
                AnimationClip ac = (AnimationClip) EditorGUILayout.ObjectField(temp, typeof(AnimationClip), false, GUILayout.ExpandWidth(true));
                EditorGUI.showMixedValue = false;
                if (ac != temp) {
                    foreach (Chip obj in serializedObject.targetObjects) {
                        int i = obj.clips_serialized.FindIndex(x => x.name == clip_name);
                        if (i >= 0)
                            obj.clips_serialized[i].clip = ac; 
                    }
                }
            } else {
                foreach (Chip obj in serializedObject.targetObjects) {
                    int i = obj.clips_serialized.FindIndex(x => x.name == clip_name);
                    if (i >= 0) {
                        temp = obj.clips_serialized[i].clip;
                        break;
                    }
                }
                AnimationClip ac = (AnimationClip) EditorGUILayout.ObjectField(temp, typeof(AnimationClip), false, GUILayout.ExpandWidth(true));
                if (ac != temp) {
                    foreach (Chip obj in serializedObject.targetObjects) {
                        int i = obj.clips_serialized.FindIndex(x => x.name == clip_name);
                        if (i >= 0)
                            obj.clips_serialized[i].clip = ac;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        #endregion

        serializedObject.ApplyModifiedProperties();
    }
}
