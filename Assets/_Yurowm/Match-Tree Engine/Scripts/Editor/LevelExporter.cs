using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using EditorUtils;
using Berry.Utils;

public class LevelExporter : MetaEditor {

    string mode = "Export";
    string file_path = "";
    bool file_exist = false;

    bool processing = false;
      
    List<LevelProfile> content = new List<LevelProfile>();
    Transform level_parent = null;
    public override void OnInspectorGUI() {
        if (processing) {
            ProcessingGUI();
            return;
        } else
            EditorUtility.ClearProgressBar();

        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(300));
        GUILayout.Label("Operation", EditorStyles.boldLabel);

        if (GUILayout.Toggle(mode == "Export", "Export", EditorStyles.radioButton, GUILayout.Width(150)) && mode != "Export")
            mode = "Export";

        if (GUILayout.Toggle(mode == "Import", "Import", EditorStyles.radioButton, GUILayout.Width(150)) && mode != "Import")
            mode = "Import";

        EditorGUILayout.EndVertical();

        if (mode == "Import") {
            EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(300));
            GUILayout.Label("File", EditorStyles.boldLabel);

            GUILayout.Label(file_path == "" ? "/..." : file_path, EditorStyles.textArea);

            
            if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(80))) {
                file_path = EditorUtility.OpenFilePanel(
                        "Select a file for import",
                        "",
                        "");
            }

            file_exist = false;
            if (file_path != "") {
                FileInfo t = new FileInfo(file_path);
                if (t.Exists) {
                    file_exist = true;
                    if (t.Extension != ".levelcatalog" && t.Extension != ".levelcatalog2") {
                        file_exist = false;
                        EditorGUILayout.HelpBox("Only files with extensions \".levelcatalog\" and \".levelcatalog2\" is suppoted", MessageType.Error);
                    }
                }
            }
		if (!file_exist)
                EditorGUILayout.HelpBox("File doesn't exist", MessageType.Error);

            EditorGUILayout.EndVertical();
        }

        content.Clear();
        Level[] levels = FindObjectsOfType<Level>();
        foreach (Level level in levels) {
            LevelProfile profile = level.profile;
            content.Add(profile);
        }

        if (mode == "Export") {
            EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(300));
            GUILayout.Label("Content", EditorStyles.boldLabel);

            GUILayout.Label(content.Count + " level(s) found", EditorStyles.miniBoldLabel);

            if (content.Count > 0) {
                if (GUILayout.Button("Export", EditorStyles.miniButton, GUILayout.Width(80))) {
                    file_path = EditorUtility.SaveFilePanel(
                        "Select a file for export",
                        "",
                        "LevelCatalog",
                        "levelcatalog2");
                    
                    if (file_path != "") 
                        EditorCoroutine.start(Export());
                }
            }
            EditorGUILayout.EndVertical();
        }

        if (mode == "Import" && file_exist) {
            EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(300));
            GUILayout.Label("Content", EditorStyles.boldLabel);

            if (level_parent == null) {
                Level level = FindObjectOfType<Level>();
                if (level)
                    level_parent = level.transform.parent;
            }


            if (levels.Length > 0) {
                EditorGUILayout.HelpBox("Before you import new levels you need to remove all levels from the scene!", MessageType.Info);
                Color color = GUI.backgroundColor;
                GUI.backgroundColor = Color.Lerp(color, Color.red, 0.7f);
                if (GUILayout.Button("Remove & Import", GUILayout.Width(200))) {
                    foreach (Level level in levels)
                        DestroyImmediate(level.gameObject);
                    if (level_parent == null)
                        level_parent = new GameObject("Levels").transform;
                    FileInfo info = new FileInfo(file_path);
                    if (info.Extension == ".levelcatalog")
                        EditorCoroutine.start(Import(true));
                    if (info.Extension == ".levelcatalog2")
                        EditorCoroutine.start(Import(false));
                }
                GUI.backgroundColor = color;
            } else {
                if (GUILayout.Button("Import", GUILayout.Width(200))) {
                    if (level_parent == null)
                        level_parent = new GameObject("Levels").transform;
                    FileInfo info = new FileInfo(file_path);
                    if (info.Extension == ".levelcatalog")
                        EditorCoroutine.start(Import(true));
                    if (info.Extension == ".levelcatalog2")
                        EditorCoroutine.start(Import(false));
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
    
    float bar_progress = 0;
    string bar_message = "";
    IEnumerator Export() {
        processing = true;
        XmlDocument document = new XmlDocument();

        document.LoadXml("<root></root>");
        XmlElement root = document.DocumentElement;

        for (int i = 0; i < content.Count; i++) {            
            root.AppendChild(LevelProfileToXML(content[i], ref document));

            bar_progress = 1f * i / content.Count;
            bar_message = Mathf.RoundToInt(bar_progress * 100).ToString() + "%";
            RepaintIt();
            yield return 0;
        }

        bar_progress = 1f ;
        bar_message = "Saving";
        RepaintIt();

        yield return 0;

        document.Save(file_path);

        yield return 0;
        processing = false;
        RepaintIt();
    }

    IEnumerator Import(bool old = false) {
        processing = true;
        XmlDocument document = new XmlDocument();
        document.Load(file_path);
        
        XmlElement root = document.DocumentElement;
        List<LevelProfile> profiles = new List<LevelProfile>();
        foreach (XmlNode node in root.ChildNodes) {
            if (node.Name == "level") {
                if (old)
                    profiles.Add(OldXmlToLevelProfile(node));
                else
                    profiles.Add(XmlToLevelProfile(node));
            }
            bar_progress = 0.33f;
            bar_message = "Reading";
            RepaintIt();
            yield return 0;
        }

        profiles.Sort((LevelProfile a, LevelProfile b) => {
            if (a.level > b.level) return 1;
            if (a.level < b.level) return -1;
            return 0;
        });

        foreach (LevelProfile profile in profiles) {
            GameObject go = new GameObject();
            go.transform.SetParent(level_parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            Level level = go.AddComponent<Level>();
            level.profile = profile;
            level.profile.levelID = level.GetInstanceID();

            bar_progress = 0.66f;
            bar_message = "Creating levels";
            RepaintIt();
            yield return 0;
        }

        bar_progress = 1f;
        bar_message = "Complete";
        RepaintIt();

        yield return 0;
        processing = false;
        RepaintIt();
    }

    XmlElement LevelProfileToXML(LevelProfile profile, ref XmlDocument document) {
        XmlElement level = document.CreateElement("level");

        level.SetAttribute("number", profile.level.ToString());

        level.SetAttribute("width", profile.width.ToString());
        level.SetAttribute("height", profile.height.ToString());

        level.SetAttribute("color_count", profile.colorCount.ToString());

        level.SetAttribute("stones", profile.stonePortion.ToString());

        level.SetAttribute("star1", profile.firstStarScore.ToString());
        level.SetAttribute("star2", profile.secondStarScore.ToString());
        level.SetAttribute("star3", profile.thirdStarScore.ToString());

        level.SetAttribute("limitation", profile.limitation.ToString());
        level.SetAttribute("limit", profile.limit.ToString());

        level.SetAttribute("target", profile.target.ToString());
        if (profile.target == FieldTarget.Color) {
            level.SetAttribute("target_color_count", profile.targetColorCount.ToString());
            level.SetAttribute("target_color_values", string.Join(",", profile.countOfEachTargetCount.Select(x => x.ToString()).ToArray()));
        }
        if (profile.target == FieldTarget.SugarDrop) {
            level.SetAttribute("target_sdrop_count", profile.targetSugarDropsCount.ToString());
        }

        foreach (SlotSettings slot in profile.slots) {
            XmlElement slot_element = document.CreateElement("slot");
            slot_element.SetAttribute("x", slot.position.x.ToString());
            slot_element.SetAttribute("y", slot.position.y.ToString());
            if (slot.generator)
                slot_element.SetAttribute("generator", "");
            if (slot.jelly_level > 0)
                slot_element.SetAttribute("jelly", slot.jelly_level.ToString());
            if (!string.IsNullOrEmpty(slot.jam))
                slot_element.SetAttribute("jam", slot.jam);
            if (!string.IsNullOrEmpty(slot.block_type)) {
                slot_element.SetAttribute("block", slot.block_type);
                slot_element.SetAttribute("block_level", slot.block_level.ToString());
            }
            if (slot.teleport != int2.Null)
                slot_element.SetAttribute("teleport", slot.teleport.x + "," + slot.teleport.y);
            slot_element.SetAttribute("gravity", slot.gravity.ToString());
            if (slot.tags.Count > 0)
                slot_element.SetAttribute("tags", string.Join(",", slot.tags.ToArray()));
            if (!string.IsNullOrEmpty(slot.chip)) {
                slot_element.SetAttribute("chip", slot.chip);
                slot_element.SetAttribute("color_id", slot.color_id.ToString());
            }

            level.AppendChild(slot_element);
        }
        foreach (int2 coord in profile.wall_horizontal) {
            XmlElement wall = document.CreateElement("wallh");
            wall.SetAttribute("x", coord.x.ToString());
            wall.SetAttribute("y", coord.y.ToString());

            level.AppendChild(wall);
        }

        foreach (int2 coord in profile.wall_vertical) {
            XmlElement wall = document.CreateElement("wallv");
            wall.SetAttribute("x", coord.x.ToString());
            wall.SetAttribute("y", coord.y.ToString());

            level.AppendChild(wall);
        }
        
        return level;
    }

    LevelProfile XmlToLevelProfile(XmlNode node) {
        LevelProfile level = new LevelProfile();

        foreach (XmlAttribute attribute in node.Attributes) {
            if (attribute.Name == "number") level.level = int.Parse(attribute.Value);
            if (attribute.Name == "width") level.width = int.Parse(attribute.Value);
            if (attribute.Name == "height") level.height = int.Parse(attribute.Value);
            if (attribute.Name == "color_count") level.colorCount = int.Parse(attribute.Value);
            if (attribute.Name == "stones") level.stonePortion = float.Parse(attribute.Value);
            if (attribute.Name == "star1") level.firstStarScore = int.Parse(attribute.Value);
            if (attribute.Name == "star2") level.secondStarScore = int.Parse(attribute.Value);
            if (attribute.Name == "star3") level.thirdStarScore = int.Parse(attribute.Value);
            if (attribute.Name == "limitation") level.limitation = (Limitation) System.Enum.Parse(typeof (Limitation), attribute.Value);
            if (attribute.Name == "limit") level.limit = int.Parse(attribute.Value);
            if (attribute.Name == "target") level.target = (FieldTarget) System.Enum.Parse(typeof(FieldTarget), attribute.Value);
            if (attribute.Name == "target_color_count") level.targetColorCount = int.Parse(attribute.Value);
            if (attribute.Name == "target_color_values") level.countOfEachTargetCount = attribute.Value.Split(',').ToArray().Select(x => int.Parse(x)).ToArray();
            if (attribute.Name == "target_sdrop_count") level.targetSugarDropsCount = int.Parse(attribute.Value);
            if (attribute.Name == "ai_difficult") level.ai_difficult = float.Parse(attribute.Value);
        }

        foreach (XmlNode child in node.ChildNodes) {
            int x = -1;
            int y = -1;
            foreach (XmlAttribute attribute in child.Attributes) {
                if (attribute.Name == "x") x = int.Parse(attribute.Value);
                if (attribute.Name == "y") y = int.Parse(attribute.Value);
            }

            if (x == -1 || y == -1) continue;

            if (child.Name == "slot") {
                SlotSettings slot = new SlotSettings(new int2(x, y));
                slot.chip = "";
                level.slots.Add(slot);
                foreach (XmlAttribute attribute in child.Attributes) {
                    if (attribute.Name == "generator") slot.generator = true;
                    if (attribute.Name == "jelly") slot.jelly_level = int.Parse(attribute.Value);
                    if (attribute.Name == "jam") slot.jam = attribute.Value;
                    if (attribute.Name == "block") slot.block_type = attribute.Value;
                    if (attribute.Name == "block_level") slot.block_level = int.Parse(attribute.Value);
                    if (attribute.Name == "teleport") {
                        int[] coord = attribute.Value.Split(',').Select(s => int.Parse(s)).ToArray();
                        slot.teleport = new int2(coord[0], coord[1]);
                    }
                    if (attribute.Name == "gravity") slot.gravity = (Side) System.Enum.Parse(typeof (Side), attribute.Value);
                    if (attribute.Name == "tags") slot.tags = attribute.Value.Split(',').ToList();
                    if (attribute.Name == "chip") slot.chip = attribute.Value;
                    if (attribute.Name == "color_id") slot.color_id = int.Parse(attribute.Value);
                }
            }
            if (child.Name == "wallh")
                level.wall_horizontal.Add(new int2(x, y));
            if (child.Name == "wallv")
                level.wall_vertical.Add(new int2(x, y));
        }
        return level;
    }

    LevelProfile OldXmlToLevelProfile(XmlNode node) {
        LevelProfile level = new LevelProfile();

        foreach (XmlAttribute attribute in node.Attributes) {
            if (attribute.Name == "number") level.level = int.Parse(attribute.Value);
            if (attribute.Name == "width") level.width = int.Parse(attribute.Value);
            if (attribute.Name == "height") level.height = int.Parse(attribute.Value);
            if (attribute.Name == "color_count") level.colorCount = int.Parse(attribute.Value);
            if (attribute.Name == "stones") level.stonePortion = float.Parse(attribute.Value);
            if (attribute.Name == "star1") level.firstStarScore = int.Parse(attribute.Value);
            if (attribute.Name == "star2") level.secondStarScore = int.Parse(attribute.Value);
            if (attribute.Name == "star3") level.thirdStarScore = int.Parse(attribute.Value);
            if (attribute.Name == "limitation") level.limitation = (Limitation) System.Enum.Parse(typeof (Limitation), attribute.Value);
            if (attribute.Name == "limit") level.limit = int.Parse(attribute.Value);
            if (attribute.Name == "target") level.target = (FieldTarget) System.Enum.Parse(typeof(FieldTarget), attribute.Value);
            if (attribute.Name == "target_color_count") level.targetColorCount = int.Parse(attribute.Value);
            if (attribute.Name == "target_color_values") level.countOfEachTargetCount = attribute.Value.Split(',').ToArray().Select(x => int.Parse(x)).ToArray();
            if (attribute.Name == "target_sdrop_count") level.targetSugarDropsCount = int.Parse(attribute.Value);
        }

        foreach (XmlNode child in node.ChildNodes) {
            int x = -1;
            int y = -1;
            foreach (XmlAttribute attribute in child.Attributes) {
                if (attribute.Name == "x") x = int.Parse(attribute.Value);
                if (attribute.Name == "y") y = int.Parse(attribute.Value);
            }
            if (x == -1 || y == -1) continue;
            y = level.height - y - 1;
            if (child.Name == "slot") {
                SlotSettings slot = new SlotSettings(new int2(x, y));
                slot.chip = "SimpleChip";
                level.slots.Add(slot);
                foreach (XmlAttribute attribute in child.Attributes) {
                    if (attribute.Name == "generator") slot.generator = true;
                    if (attribute.Name == "jelly") slot.jelly_level = int.Parse(attribute.Value);
                    if (attribute.Name == "block") {
                        int id = int.Parse(attribute.Value);
                        if (id > 0 && id <= 3) {
                            slot.block_type = "Block";
                            slot.block_level = id;
                            slot.chip = "";
                        }
                        if (id == 4) {
                            slot.block_type = "Weed";
                            slot.chip = "";
                        }
                        if (id == 5)
                            slot.block_type = "Branch";
                    }
                    if (attribute.Name == "teleport") {
                        int id = int.Parse(attribute.Value) - 1;
                        slot.teleport = new int2(0, 0);
                        slot.teleport.y = level.height - Mathf.FloorToInt(1f * id / 12) - 1;
                        slot.teleport.x = id % 12;
                    }

                    if (attribute.Name == "gravity") {
                        switch (int.Parse(attribute.Value)) {
                            case 0: slot.gravity = Side.Bottom; break;
                            case 1: slot.gravity = Side.Left; break;
                            case 2: slot.gravity = Side.Top; break;
                            case 3: slot.gravity = Side.Right; break;
                        }
                    }

                    if (attribute.Name == "sugar_drop")
                        slot.tags.Add("SugarDrop");
                    if (attribute.Name == "chip") {
                        int id = int.Parse(attribute.Value);

                        slot.color_id = id;
                        if (id == -1)
                            slot.chip = "";

                        if (id == 9) {
                            slot.color_id = Chip.uncoloredId;
                            slot.chip = "Stone";
                        }
                    }
                    if (attribute.Name == "powerup") {
                        switch (int.Parse(attribute.Value)) {
                            case 1: slot.chip = "CrossBomb"; break;
                            case 2: slot.chip = "SimpleBomb"; break;
                            case 3: slot.chip = "ColorBomb"; break;
                            case 4: slot.chip = "RaindowBomb"; break;
                            case 5: slot.chip = "Ladybird"; break;
                            case 6: slot.chip = "HLineBomb"; break;
                            case 7: slot.chip = "VLineBomb"; break;
                            case 8: slot.chip = "UltraColorBomb"; break;
                        }
                    }
                }
            }
            if (child.Name == "wallh")
                level.wall_horizontal.Add(new int2(x, y));
            if (child.Name == "wallv")
                level.wall_vertical.Add(new int2(x + 1, y));
        }
        return level;
    }

    void ProcessingGUI() {
        EditorUtility.DisplayProgressBar("Exporting", bar_message, bar_progress);
    }

    public override Object FindTarget() {
        return null;
    }
}

