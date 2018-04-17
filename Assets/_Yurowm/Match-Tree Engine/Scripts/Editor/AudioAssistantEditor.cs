using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;
using EditorUtils;

[CustomEditor(typeof(AudioAssistant))]
public class AudioAssistantEditor : MetaEditor {

    AudioAssistant main;
    AudioAssistant.Sound edit = null;


    AnimBool iapsFade = new AnimBool(false);
    AnimBool tracksFade = new AnimBool(false);

    public override void OnInspectorGUI() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("AudioAssistant is missing", MessageType.Error);
            return;
        }
        main = (AudioAssistant) metaTarget;
        Undo.RecordObject(main, "");

        main.musicVolume = EditorGUILayout.Slider("Music Volume", main.musicVolume, 0f, 1f);

        if (main.tracks == null)
            main.tracks = new List<AudioAssistant.MusicTrack>();

        if (main.sounds == null)
            main.sounds = new List<AudioAssistant.Sound>();
        
        #region Music Tracks
        tracksFade.target = GUILayout.Toggle(tracksFade.target, "Music Tracks", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(tracksFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("Name", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            GUILayout.Label("Audio Clip", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            foreach (AudioAssistant.MusicTrack track in main.tracks) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    main.tracks.Remove(track);
                    break;
                }
                track.name = EditorGUILayout.TextField(track.name, GUILayout.Width(100));
                track.track = (AudioClip) EditorGUILayout.ObjectField(track.track, typeof(AudioClip), false, GUILayout.ExpandWidth(true));
                
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add", GUILayout.Width(60)))
                main.tracks.Add(new AudioAssistant.MusicTrack());
          
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        #region Sounds
        iapsFade.target = GUILayout.Toggle(iapsFade.target, "Sounds", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(iapsFade.faded)) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("Edit", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(40));
            GUILayout.Label("Name", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(120));
            GUILayout.Label("Audio Clips", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            foreach (AudioAssistant.Sound sound in main.sounds) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    main.sounds.Remove(sound);
                    break;
                }
                if (GUILayout.Button("Edit", GUILayout.Width(40))) {
                    if (edit == sound)
                        edit = null;
                    else
                        edit = sound;
                }

                sound.name = EditorGUILayout.TextField(sound.name, GUILayout.Width(120));

                if (edit == sound || sound.clips.Count == 0) {
                    EditorGUILayout.BeginVertical();
                    for (int i = 0; i < sound.clips.Count; i++) {
                        sound.clips[i] = (AudioClip) EditorGUILayout.ObjectField(sound.clips[i], typeof(AudioClip), false, GUILayout.ExpandWidth(true));
                        if (sound.clips[i] == null) {
                            sound.clips.RemoveAt(i);
                           break;
                        }
                    }
                    AudioClip new_clip = (AudioClip) EditorGUILayout.ObjectField(null, typeof(AudioClip), false, GUILayout.Width(150));
                    if (new_clip)
                        sound.clips.Add(new_clip);
                    EditorGUILayout.EndVertical();
                } else {
                    GUILayout.Label(sound.clips.Count.ToString() + " audio clip(s)", EditorStyles.miniBoldLabel);
                }


                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add", GUILayout.Width(60))) {
                main.sounds.Add(new AudioAssistant.Sound());
                edit = main.sounds[main.sounds.Count - 1];
            }
            if (GUILayout.Button("Sort", GUILayout.Width(60))) {
                main.sounds.Sort((AudioAssistant.Sound a, AudioAssistant.Sound b) => {
                    return string.Compare(a.name, b.name);
                });
                foreach (AudioAssistant.Sound sound in main.sounds)
                    sound.clips.Sort((AudioClip a, AudioClip b) => {
                        return string.Compare(a.ToString(), b.ToString());
                    });
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion
    }

    public override Object FindTarget() {
        if (AudioAssistant.main == null)
            AudioAssistant.main = FindObjectOfType<AudioAssistant>();
        return AudioAssistant.main;
    }

    public AudioAssistantEditor() {
        tracksFade.valueChanged.AddListener(RepaintIt);
        iapsFade.valueChanged.AddListener(RepaintIt);
    }
}
