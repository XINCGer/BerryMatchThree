using UnityEngine;
using System.Collections;
using EditorUtils;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Berry.Contact;
using System.Text.RegularExpressions;

public class ProjectParametersEditor : MetaEditor {
    public override Object FindTarget() {
        if (ProjectParameters.main == null)
            ProjectParameters.main = FindObjectOfType<ProjectParameters>();
        return ProjectParameters.main;
    }

    public override void OnInspectorGUI() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("SessionAssistant is missing", MessageType.Error);
            return;
        }

        if (ProfileAssistant.main == null)
            ProfileAssistant.main = FindObjectOfType<ProfileAssistant>();
        if (ProfileAssistant.main == null) {
            EditorGUILayout.HelpBox("ProfileAssistant is missing", MessageType.Error);
            return;
        }

        ProjectParameters main = (ProjectParameters) metaTarget;
        Undo.RecordObject(main, "");

        main.square_combination = EditorGUILayout.Toggle("Square Combinations", main.square_combination);
        ProfileAssistant.main.firstStartMenuSkiping = EditorGUILayout.Toggle("Skip menu on first start", ProfileAssistant.main.firstStartMenuSkiping);

        EditorGUILayout.Space();
        main.chip_acceleration = EditorGUILayout.Slider("Chip Acceleration", main.chip_acceleration, 1f, 100f);
        main.chip_max_velocity = EditorGUILayout.Slider("Chip Velocity Limit", main.chip_max_velocity, 5f, 100f);
        main.swap_duration = EditorGUILayout.Slider("Swap Duration", main.swap_duration, 0.01f, 1f);

        EditorGUILayout.Space();
        main.lifes_limit = EditorGUILayout.IntField("Lifes Limit", Mathf.Clamp(main.lifes_limit, 1, 999));
        main.refilling_time = Mathf.RoundToInt(EditorGUILayout.Slider("Life Refilling Hour (" + Mathf.FloorToInt(1f * main.refilling_time / 60).ToString("D2") + ":" + (main.refilling_time % 60).ToString("D2") + ")", main.refilling_time, 10, 24 * 60));
        main.dailyreward_hour = Mathf.RoundToInt(EditorGUILayout.Slider("Daily Reward Hour (" + main.dailyreward_hour.ToString("D2") + ":00)", main.dailyreward_hour, 00, 23));

        EditorGUILayout.Space();
        main.slot_offset = EditorGUILayout.Slider("Slot Offset", main.slot_offset, 0.01f, 2f);

        EditorGUILayout.Space();
        main.music_volume_max = EditorGUILayout.Slider("Max Music Volume", main.music_volume_max, 0f, 1f);
        
        EditorGUILayout.Space();
        main.ios_AppID = EditorGUILayout.TextField("iOS AppID", main.ios_AppID);

        EditorGUILayout.Space();
    }
}

public class SpinWheelEditor : MetaEditor {
    public override Object FindTarget() {
        if (ProjectParameters.main == null)
            ProjectParameters.main = FindObjectOfType<ProjectParameters>();
        return ProjectParameters.main;
    }

    public override void OnInspectorGUI() {
        if (!BerryStoreAssistant.main)
            BerryStoreAssistant.main = FindObjectOfType<BerryStoreAssistant>();
        if (!BerryStoreAssistant.main) {
            EditorGUILayout.HelpBox("BerryStoreAssistant is missing", MessageType.Error);
            return;
        }

        if (!metaTarget) {
            EditorGUILayout.HelpBox("SessionAssistant is missing", MessageType.Error);
            return;
        }

        ProjectParameters main = (ProjectParameters) metaTarget;
        Undo.RecordObject(main, "");

        List<string> items = BerryStoreAssistant.main.items.Select(x => x.id).ToList();
        items.Add("<remove>");
        if (main.spinWheelRewards == null || main.spinWheelRewards.Count != 8) {
            main.spinWheelRewards = new List<SpinWheelReward>();
            for (int i = 0; i < 8; i++)
                main.spinWheelRewards.Add(new SpinWheelReward());
        }
        int total_probability = 0;
        foreach (SpinWheelReward reward in main.spinWheelRewards)
            total_probability += reward.probability;

        for (int i = 0; i < 8; i++) {
            EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
            #region Icon
            EditorGUILayout.BeginVertical();
            Rect rect = EditorGUILayout.GetControlRect(false, 70, GUILayout.Width(70));
            Texture2D texture;
            if (main.spinWheelRewards[i].icon == null)
                texture = Texture2D.blackTexture;
            else
                texture = main.spinWheelRewards[i].icon.texture;
            EditorGUI.DrawTextureTransparent(rect, texture);
            main.spinWheelRewards[i].icon = (Sprite) EditorGUILayout.ObjectField(main.spinWheelRewards[i].icon, typeof(Sprite), false, GUILayout.Width(70));
            EditorGUILayout.EndVertical();
            #endregion

            Color def_color = GUI.backgroundColor;
            GUI.backgroundColor = Color.Lerp(GUI.backgroundColor, Color.yellow, 0.2f);
            EditorGUILayout.BeginVertical();
            GUI.backgroundColor = def_color;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(i.ToString() + ")", EditorStyles.boldLabel, GUILayout.Width(20));
            EditorGUILayout.LabelField("Probability (" + (100f * main.spinWheelRewards[i].probability / total_probability).ToString("F2") + "%)", GUILayout.Width(130));
            main.spinWheelRewards[i].probability = Mathf.RoundToInt(EditorGUILayout.Slider(main.spinWheelRewards[i].probability, 1, 1000));
            EditorGUILayout.EndHorizontal();
            if (main.spinWheelRewards[i].items.Count == 0)
                main.spinWheelRewards[i].items.Add(items[0] + ":" + 1);
            string id = "";
            int count = 0;
            string[] args;
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j <= main.spinWheelRewards[i].items.Count; j++) {
                if ((j + 1) % 3 == 1) {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                }
                if (j < main.spinWheelRewards[i].items.Count) {
                    args = main.spinWheelRewards[i].items[j].Split(':');
                    id = args[0];
                    count = int.Parse(args[1]);
                    int a = items.IndexOf(id);
                    if (a == -1)
                        a = 0;
                    id = items[EditorGUILayout.Popup(a, items.ToArray(), GUILayout.Width(80))];
                    count = EditorGUILayout.IntField(Mathf.Max(1, count), GUILayout.Width(30));
                    if (id == "<remove>") {
                        main.spinWheelRewards[i].items.RemoveAt(j);
                        break;
                    }
                    main.spinWheelRewards[i].items[j] = id + ":" + count;
                } else {
                    if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(30))) {
                        main.spinWheelRewards[i].items.Add(items[0] + ":" + 1);
                        break;
                    }

                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}

public class ComboFeedbackEditor : MetaEditor {
    public override Object FindTarget() {
        if (ProjectParameters.main == null)
            ProjectParameters.main = FindObjectOfType<ProjectParameters>();
        return ProjectParameters.main;
    }

    public override void OnInspectorGUI() {
        if (!AudioAssistant.main)
            AudioAssistant.main = FindObjectOfType<AudioAssistant>();
        if (!AudioAssistant.main) {
            EditorGUILayout.HelpBox("AudioAssistant is missing", MessageType.Error);
            return;
        }

        if (!metaTarget) {
            EditorGUILayout.HelpBox("SessionAssistant is missing", MessageType.Error);
            return;
        }

        ProjectParameters main = (ProjectParameters) metaTarget;
        Undo.RecordObject(main, "");

        List<string> sounds = AudioAssistant.main.sounds.Select(x => x.name).ToList();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.Label("Text", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));
        GUILayout.Label("Clip name", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        foreach (ComboFeedback.Feedback feedback in main.feedbacks) {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X", GUILayout.Width(30))) {
                main.feedbacks.Remove(feedback);
                break;
            }
            feedback.text = GUILayout.TextField(feedback.text, GUILayout.Width(150));
            int id = sounds.IndexOf(feedback.audioClipName);
            if (id < 0) id = 0;
            id = EditorGUILayout.Popup(id, sounds.ToArray(), GUILayout.Width(150));
            feedback.audioClipName = sounds[id];
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add", GUILayout.Width(70)))
            main.feedbacks.Add(new ComboFeedback.Feedback());
    }
}

public class ContactForm : MetaEditor {
    const string message = "My name: {0}\nReply to: {1}\nMy invoice: {2} ({3})\n\nMessage:\n{4}\n\n------------\n\n{5}";

    public enum AppealType {
        BugReport,
        Support,
        Other
    }

    public enum AssetProvider {
        UnityAssetStore,
        SellMyApp
    }

    PrefVariable type = new PrefVariable("ContactForm_AppealType");
    PrefVariable provider = new PrefVariable("ContactForm_AssetProvider");
    PrefVariable myName = new PrefVariable("ContactForm_Name");
    PrefVariable myEmail = new PrefVariable("ContactForm_Email");
    PrefVariable myInvoice = new PrefVariable("ContactForm_Invoice");

    PrefVariable subject = new PrefVariable("ContactForm_Subject");
    PrefVariable body = new PrefVariable("ContactForm_Body");
    PrefVariable log = new PrefVariable("ContactForm_Log");
    PrefVariable attachments = new PrefVariable("ContactForm_Attachments");

    public override void OnInspectorGUI() {
        bool isSending = Contact.IsSending();
        bool isValidate = true;
        Regex regex;

        GUI.enabled = !isSending;

        EditorGUILayout.BeginVertical();

        type.Int = (int) (AppealType) EditorGUILayout.EnumPopup("Appeal Type:", (AppealType) type.Int, GUILayout.ExpandWidth(true));
        provider.Int = (int) (AssetProvider) EditorGUILayout.EnumPopup("Asset Provider:", (AssetProvider) provider.Int, GUILayout.ExpandWidth(true));

        EditorGUILayout.Space();

        myName.String = EditorGUILayout.TextField("Name", myName.String, GUILayout.ExpandWidth(true));
        regex = new Regex(@"[\w]{2,}");
        if (!regex.IsMatch(myName.String)) {
            DrawError("Type your name");
            isValidate = false;
        }

        myEmail.String = EditorGUILayout.TextField("Email", myEmail.String, GUILayout.ExpandWidth(true));
        regex = new Regex(@"^([\w\.\-\+]+)@([\w\-]+)((\.(\w){2,3})+)$");
        if (!regex.IsMatch(myEmail.String)) {
            DrawError("Type your email (format: yourname@domain.com)");
            isValidate = false;
        }

        myInvoice.String = EditorGUILayout.TextField("Invoice (Order) number", myInvoice.String, GUILayout.ExpandWidth(true));

        int invoice_format = 0;
        switch ((AssetProvider) provider.Int) {
            case AssetProvider.SellMyApp: invoice_format = 5; break;
            case AssetProvider.UnityAssetStore: invoice_format = 9; break;
        }

        regex = new Regex(@"\A[\d]{" + invoice_format + @"}\Z");
        if (!regex.IsMatch(myInvoice.String)) {
            DrawError("Type your invoice number (" + invoice_format + " digits)");
            isValidate = false;
        }

        EditorGUILayout.Space();

        subject.String = EditorGUILayout.TextField("Subject", subject.String, GUILayout.ExpandWidth(true));

        GUILayout.Label("Message", GUILayout.Width(300));
        body.String = EditorGUILayout.TextArea(body.String, GUI.skin.textArea, GUILayout.ExpandWidth(true), GUILayout.Height(300));
        if (body.String.Length == 0) {
            DrawError("Type a message");
            isValidate = false;
        }

        if (type.Int == (int) AppealType.BugReport) {
            GUILayout.Label("Logs or another technical information", GUILayout.Width(300));
            log.String = EditorGUILayout.TextArea(log.String, GUI.skin.textArea, GUILayout.ExpandWidth(true), GUILayout.Height(100));

            EditorGUILayout.Space();
        }

        List<string> fileList = null;
        if (type.Int == (int) AppealType.BugReport || type.Int == (int) AppealType.Support) {
            fileList = new List<string>();
            if (!string.IsNullOrEmpty(attachments.String))
                fileList = attachments.String.Split(';').ToList();
            foreach (string file in fileList) {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(30))) {
                    fileList.Remove(file);
                    break;
                }
                GUILayout.Label(new System.IO.FileInfo(file).Name, EditorStyles.miniLabel, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
            }
            if (fileList.Count < 5) {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add attachment", EditorStyles.miniButton, GUILayout.Width(100))) {
                    string path = EditorUtility.OpenFilePanel("Select file", "", "");
                    if (path.Length > 0)
                        fileList.Add(path);
                }
                EditorGUILayout.EndHorizontal();
            }
            attachments.String = string.Join(";", fileList.ToArray());
        }
        GUI.enabled = !isSending && isValidate && !EditorApplication.isCompiling;
        
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (isSending)
            GUILayout.Label("Sending...", GUILayout.Width(90));
        if (EditorApplication.isCompiling)
            GUILayout.Label("Compiling...", GUILayout.Width(90));
        GUILayout.FlexibleSpace();
        if (Contact.IsSending()) {
            bool active = GUI.enabled;
            GUI.enabled = true;
            if (GUILayout.Button("Break", GUILayout.Width(70)))
                Contact.Break();
            GUI.enabled = active;
        } else {
            if (GUILayout.Button("Send", GUILayout.Width(70))) {
                EditorGUI.FocusTextInControl("");
                if (string.IsNullOrEmpty(subject.String))
                    subject.String = "No Subject";
                Contact.Send(
                    myName.String,
                    ((AppealType) type.Int).ToString() + ": " + subject.String,
                    string.Format(message, myName.String, myEmail.String, myInvoice.String, (AssetProvider) provider.Int, body.String, log.String),
                    OnSent,
                    fileList
                    );
            }
        }
        EditorGUILayout.EndHorizontal();

        foreach (string error in Contact.GetErrors())
            DrawError(error);

        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }

    void DrawError(string error) {
        Color color = GUI.color;
        GUI.color = Color.red;
        GUILayout.Label(error, EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
        GUI.color = color;
    }

    void OnSent() {
        EditorCoroutine.start(ClearOnSent());
    }

    IEnumerator ClearOnSent() {
        body.String = "";
        subject.String = "";
        attachments.String = "";
        log.String = "";
        RepaintIt();
        EditorUtility.DisplayDialog("Contact", "Your email has been sent", "Ok");
        yield break;
    }

    public override Object FindTarget() {
        return null;
    }
}

public class PRORequired : MetaEditor {
    public override Object FindTarget() {
        return null;
    }

    #region Styles
    GUIStyle _bgStyle = null;
    GUIStyle bgStyle {
        get {
            if (_bgStyle == null) {
                _bgStyle = new GUIStyle(EditorStyles.textArea);
                _bgStyle.normal.background = Texture2D.whiteTexture;
                _bgStyle.padding = new RectOffset(0, 0, 0, 0);
                _bgStyle.margin = new RectOffset(0, 0, 0, 0);
            }
            return _bgStyle;
        }
    }

    GUIStyle _titleStyle = null;
    GUIStyle titleStyle {
        get {
            if (_titleStyle == null) {
                _titleStyle = new GUIStyle(EditorStyles.largeLabel);
                _titleStyle.normal.textColor = Color.black;
                _titleStyle.fontStyle = FontStyle.Bold;
                _titleStyle.alignment = TextAnchor.MiddleCenter;
            }
            return _titleStyle;
        }
    }

    GUIStyle _textStyle = null;
    GUIStyle textStyle {
        get {
            if (_textStyle == null) {
                _textStyle = new GUIStyle(EditorStyles.label);
                _textStyle.normal.textColor = Color.black;
                _textStyle.alignment = TextAnchor.UpperLeft;
                _textStyle.wordWrap = true;
            }
            return _textStyle;
        }
    }

    GUIStyle _buttonStyle = null;
    GUIStyle buttonStyle {
        get {
            if (_buttonStyle == null) {
                _buttonStyle = new GUIStyle(GUI.skin.FindStyle("Button"));
                _buttonStyle.fontSize = 20;

                _buttonStyle.normal.background = new Texture2D(1, 1);
                _buttonStyle.normal.background.SetPixel(0, 0, new Color(103f / 256, 194f / 256, 116f / 256, 1));
                _buttonStyle.normal.background.Apply();
                _buttonStyle.normal.textColor = Color.white;

                _buttonStyle.hover = _buttonStyle.hover;

                _buttonStyle.active.background = new Texture2D(1, 1);
                _buttonStyle.active.background.SetPixel(0, 0, new Color(62f / 256, 130f / 256, 73f / 256, 1));
                _buttonStyle.active.background.Apply();
                _buttonStyle.active.textColor = Color.white;


                _buttonStyle.alignment = TextAnchor.MiddleCenter;
                _buttonStyle.wordWrap = true;
            }
            return _buttonStyle;
        }
    }
    #endregion

    #region Images
    Dictionary<string, Texture> images = new Dictionary<string, Texture>();
    string[] imageNames = new string[] {
        "HeaderLogo", "DuelWithTed", "AdsLogos",
        "CutScenes", "Syncing", "Leaderboard",
        "Notifications", "Facebook"
    };
    #endregion

    void OnEnable() {
        images = new Dictionary<string, Texture>();
        foreach (string _name in imageNames)
            images.Add(_name, EditorGUIUtility.Load("PRO/" + _name + ".png") as Texture);
    }

    public override void OnInspectorGUI() {
        Color bgcolor = GUI.backgroundColor;
        Color color = GUI.color;

        GUI.backgroundColor = Color.white;
        EditorGUILayout.BeginVertical(bgStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        GUILayout.Label("This feature avaliable only in PRO version", titleStyle);

        EditorGUILayout.Space();
        DrawImage("HeaderLogo");
        EditorGUILayout.Space();

        #region Duel with Ted
        GUILayout.Label("Duel with Ted", titleStyle);
        GUILayout.Label("\"Duel\" is an exclusive gameplay mode, where the player will need to fill the level with jam like in the Jam mode. But he/she will compete with AI (one of in-game characters is Ted). It is really funny game play mode.", textStyle);
        DrawImage("DuelWithTed");
        GUILayout.Space(20);
        #endregion

        #region Cut-scenes
        GUILayout.Label("Cut-Scenes", titleStyle);
        GUILayout.Label("You may write a short dialog between the two players that will play back every time you launch a pre-selected level. This allows to introduce exciting tutorial moments into the game as well as enrich the game with a plot.", textStyle);
        DrawImage("CutScenes");
        GUILayout.Space(20);
        #endregion

        #region Leaderboards
        GUILayout.Label("Leaderboards", titleStyle);
        GUILayout.Label("Each level has its own list of leaders, where the player may compete with his Facebook friends as well as with fake players from the Berry Match-Three universe.", textStyle);
        DrawImage("Leaderboard");
        GUILayout.Space(20);
        #endregion

        #region Facebook
        GUILayout.Label("Facebook SDK", titleStyle);
        GUILayout.Label("The game uses the Facebook SDK for syncing user profiles. Also this SDK allows to share scores and rewards as well as invite player's friends into the game. This is a great virality driver for your game!", textStyle);
        DrawImage("Facebook");
        GUILayout.Space(20);
        #endregion

        #region Local Notifications
        GUILayout.Label("Local Notifications", titleStyle);
        GUILayout.Label("The player will always know it’s time to play again. Each time when the player’s lives will be refilled, he will receive a message on his/her telephone. This concerns the daily rewards too. Also, the game will wish the player good morning if he ever forgets to play it :)", textStyle);
        DrawImage("Notifications");
        GUILayout.Space(20);
        #endregion

        #region Syncing
        GUILayout.Label("Syncing", titleStyle);
        GUILayout.Label("By player’s wish he can connect the game to his Facebook profile and save the progress in the cloud and, as a result, play on multiple devices. This is made possible by the Backendless cloud database service.", textStyle);
        DrawImage("Syncing");
        GUILayout.Space(20);
        #endregion

        #region Ads Networks
        GUILayout.Label("Ads Networks", titleStyle);
        GUILayout.Label("The PRO version contains built-in advertisement networks: Chartboost, UnityAds, AdColony and AdMob. They allow to monetize your application by means of showing video ads. There is also a way to reward the player for viewing an extra ad video.", textStyle);
        DrawImage("AdsLogos");
        GUILayout.Space(20);
        #endregion
        
        #region Buttons
        GUILayout.Space(30);
        float buttonWidth = 150;
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(70));
        Rect buttonRect = new Rect(rect);
        buttonRect.x = rect.width / 2 - buttonWidth - 5;
        buttonRect.width = buttonWidth;
        if (GUI.Button(buttonRect, "TRY DEMO", buttonStyle))
            Application.OpenURL("https://" + "play.google.com/store/apps/details?id=com.yurowm.berrymatchpro");

        buttonRect.x = rect.width / 2 + 5;
        if (GUI.Button(buttonRect, "GET PRO", buttonStyle))
            Application.OpenURL("https://" + "www.sellmyapp.com/downloads/berry-match-three-pro/");
        GUILayout.Space(100);
        #endregion

        EditorGUILayout.EndVertical();
    }

    void DrawImage(string _name) {
        Texture texture = images[_name];
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(texture.height));
        float width = Mathf.Min(rect.width, texture.width);
        rect.x = rect.width / 2 - width / 2;
        rect.width = width;
        GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
    }
}