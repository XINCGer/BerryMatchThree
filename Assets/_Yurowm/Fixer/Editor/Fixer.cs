using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using Mono.CSharp;
using EditorUtils;
using Berry.Contact;
using System.Threading;

namespace Berry.Fixer {
    public class Fixer : MetaEditor {

        #region VersionInfo
        /* Update Info:
            min_project_version:bmte4.2;
        */
        const string project = "bmte";
        const string sub = "standard";
        const float project_version = 4.20f;
        const float fixer_version = 1.2f;
        #endregion

        string issuesLink = "http://" + "jellymobile.net/jellymobile.net/yurowm/issues/" + project + "/v" + project_version.ToString("F1") + "/{0}issue{1}.xml";
        string updateLink = "http://" + "jellymobile.net/jellymobile.net/yurowm/fixer/FixerUpdate.cs";
        public static PrefVariable cache = new PrefVariable("FixerCache");
        public static PrefVariable alias = new PrefVariable("FixerAlias");
        static string dataPath;

        void OnEnable() {
            dataPath = Application.dataPath;
            if (!CheckSubproject())
                return;
            DownloadIssues();
        }

        ~Fixer() {
            StopDownloadIssues();
        }

        bool CheckSubproject() {
            #pragma warning disable CS0162 // Unreachable code detected
            if (sub != "")
                return true;

            string result = "";

            if (project == "bmte") {
                List<FileInfo> files = SearchFiles(dataPath);
                FileInfo adAssistant = files.Find(x => x.Name == "AdAssistant.cs");
                if (adAssistant != null)
                    result = "pro";
                else
                    result = "standard";
            }

            if (result != "") {
                FileInfo fileInfo = SearchFiles(dataPath).Find(x => x.Name == "Fixer.cs");

                if (fileInfo != null) {
                    StreamReader fileR = new StreamReader(fileInfo.FullName);
                    string code = fileR.ReadToEnd();
                    fileR.Close();

                    Regex re = new Regex(@"const\s+string\s+sub\s+\=\s+""{2}\;");
                    code = re.Replace(code, "const string sub = \"" + result + "\";");

                    StreamWriter fileW = new StreamWriter(fileInfo.FullName);
                    fileW.WriteLine(code);
                    fileW.Close();

                    StopDownloadIssues();
                    AssetDatabase.Refresh();
                }
            }
            return false;
            #pragma warning restore CS0162 // Unreachable code detected
        }

        float new_fixer_version = 0;
        string new_fixer = "";
        bool update_checked = false;
        List<string> issueTypes = new List<string>();
        int selected_type = 0;

        IEnumerator DownloadUpdate() {
            update_checked = true;
            WWW data = new WWW(updateLink);
            while (!data.isDone)
                yield return 0;

            if (!string.IsNullOrEmpty(data.error)) {
                update_checked = false;
                yield break;
            }

            Regex re = new Regex(@"#region\s+VersionInfo\s+\/\*\s*Update\s+Info:\s*(?<info>[\S\s]*?)\*\/(?:[\S\s])*?#endregion");
            Match version_info = re.Match(data.text);
            if (version_info != null) {
                string info = version_info.Groups["info"].Value.Trim();
                re = new Regex(@"min_project_version:bmte(?<min_version>\d+\.?\d*);");
                Match min_version = re.Match(info);
                if (min_version != null) {
                    string _min_version = min_version.Groups["min_version"].Value;
                    if (project_version < float.Parse(_min_version)) {
                        new_fixer_version = fixer_version;
                        yield break;
                    }
                }
            }

            re = new Regex(@"fixer_version\s*=\s*(?<version>\d+\.?\d*)f\s*;");
            try {
                Match version_line = re.Match(data.text);
                new_fixer_version = float.Parse(version_line.Groups["version"].Value);
            } catch (System.Exception) {
                update_checked = false;
                yield break;
            }

            if (new_fixer_version <= fixer_version)
                yield break;

            new_fixer = data.text;

            re = new Regex(@"project\s*=\s*"".*""\s*;");
            new_fixer = re.Replace(new_fixer, "project = \"" + project + "\";");

            re = new Regex(@"project_version\s*=\s*(?<version>\d+\.?\d*)f\s*;");
            new_fixer = re.Replace(new_fixer, "project_version = " + project_version.ToString("F2") + "f;");

            re = new Regex(@"const\s+string\s+sub\s+\=\s+""[\S\s]*?""\;");
            new_fixer = re.Replace(new_fixer, "const string sub = \"\";");
        }

        static List<FileInfo> SearchFiles(string directory) {
            List<FileInfo> result = new List<FileInfo>();
            result.AddRange(new DirectoryInfo(directory).GetFiles().ToList());
            foreach (DirectoryInfo dir in new DirectoryInfo(directory).GetDirectories())
                result.AddRange(SearchFiles(dir.FullName));
            return result;
        }

        void InstallUpdate() {
            // Search Fixer.cs file
            FileInfo fileInfo = SearchFiles(dataPath).Find(x => x.Name == "Fixer.cs");

            if (fileInfo == null)
                return;

            // Write new code
            StreamWriter fileW = new StreamWriter(fileInfo.FullName);
            fileW.WriteLine(new_fixer);
            fileW.Close();

            // Recompile solution
            StopDownloadIssues();
            AssetDatabase.Refresh();
        }

        #region Issue Downloader
        void DownloadIssues() {
            StopDownloadIssues();
            downloader = EditorCoroutine.start(DownloadIssuesRoutine());
        }

        void StopDownloadIssues() {
            if (downloader != null && downloader.IsPlaying()) {
                downloader.stop();
                downloader = null;
            }
        }

        static EditorCoroutine downloader;
        IEnumerator DownloadIssuesRoutine() {
            if (EditorApplication.isPlaying)
                yield break;

            issueTypes.Clear();
            issueTypes.Add("All Types");
            selected_type = 0;

            List<int> _cache = new List<int>();
            if (!string.IsNullOrEmpty(cache.String))
                _cache = cache.String.Split(';').Select(x => int.Parse(x)).ToList();

            List<string> aliases = new List<string>();
            aliases.Add("");
            if (!string.IsNullOrEmpty(alias.String))
                aliases.AddRange(alias.String.Split(',').Select(x => x.Trim()).ToList());

            int fileNumber;
            Issue.all.Clear();
            foreach (string _alias in aliases) {
                if (Issue.all.ContainsKey(_alias))
                    continue;
                Issue.all.Add(_alias, new List<Issue>());
                fileNumber = 0;
                while (true) {
                    if (fileNumber == -1)
                        break;
                    WWW[] datas = new WWW[] {
                        new WWW(string.Format(issuesLink, string.IsNullOrEmpty(_alias) ? "" : (_alias + "/"), fileNumber)),
                        new WWW(string.Format(issuesLink, string.IsNullOrEmpty(_alias) ? "" : (_alias + "/"), fileNumber + 1)),
                        new WWW(string.Format(issuesLink, string.IsNullOrEmpty(_alias) ? "" : (_alias + "/"), fileNumber + 2))
                    };

                    foreach (WWW data in datas) {
                        while (!data.isDone) {
                            if (EditorApplication.isCompiling)
                                yield break;
                            yield return new WaitForSeconds(0.3f);
                        }

                        if (string.IsNullOrEmpty(data.error)) {
                            Issue issue = XMLtoIssue(data.text);
                            if (issue.subproject.Contains(sub)) {
                                issue.number = fileNumber;
                                Issue.all[_alias].Add(issue);
                                if (_cache.Contains((issue.cacheCode).GetHashCode()))
                                    issue.isCached = true;

                                if (!issueTypes.Contains(issue.type))
                                    issueTypes.Add(issue.type);

                                issue.alias = _alias;
                                RepaintIt();
                            }
                            fileNumber++;
                        } else
                            fileNumber = -1;
                    }
                }
            }

            RepaintIt();
        }

        Issue XMLtoIssue(string text) {
            XmlDocument document = new XmlDocument();

            Regex re;
            foreach (string tag in new string[] { "original", "replacement", "checker", "fixer" }) {

                re = new Regex(@"\<" + tag + @"\>(?<code>[\S\s]*?)\<\/" + tag + @"\>");
                foreach (Match match in re.Matches(text)) {
                    string origianl = match.Groups["code"].Value;
                    string result = origianl;
                    result = result.Replace("&", "&amp;");
                    result = result.Replace("<", "&lt;");
                    result = result.Replace(">", "&gt;");
                    result = result.Replace("\"", "&quot;");
                    result = result.Replace("'", "&apos;");
                    text = text.Replace(origianl, result);
                }
            }

            document.LoadXml(text);

            Issue issue = new Issue();
            XmlNode root = document.ChildNodes[0];
            foreach (XmlAttribute attribute in root.Attributes) {
                if (attribute.Name == "type")
                    issue.type = attribute.Value;
                if (attribute.Name == "description")
                    issue.desription = attribute.Value;
                if (attribute.Name == "minVersion")
                    issue.minVersion = float.Parse(attribute.Value);
                if (attribute.Name == "required" && attribute.Value != "")
                    issue.required = attribute.Value.Split(',').Select(x => int.Parse(x)).ToList();
                if (attribute.Name == "subproject" && attribute.Value != "")
                    issue.subproject = attribute.Value.Split(',').Select(x => x.Trim()).ToList();

            }

            foreach (XmlNode element in root.ChildNodes) {
                if (element.Name == "checker")
                    issue.checker = element.InnerText;
                if (element.Name == "fixer")
                    issue.fixer = element.InnerText;
                if (element.Name == "codefix") {
                    Issue.Codefix codefix = new Issue.Codefix();
                    foreach (XmlAttribute attribute in element.Attributes) {
                        if (attribute.Name == "file")
                            codefix.filename = attribute.Value;
                    }
                    foreach (XmlNode code in element.ChildNodes) {
                        if (code.Name == "original")
                            codefix.original = code.InnerText;
                        if (code.Name == "replacement")
                            codefix.replacement = code.InnerText;
                    }
                    if (codefix.original != "" && codefix.replacement != "" && codefix.filename != "")
                        issue.codefixes.Add(codefix);
                }
            }
            issue.cacheCode = Mathf.Abs((text + dataPath).GetHashCode());
            issue.Initialize();
            return issue;
        }
        #endregion

        enum Status { All, Unchecked, Checked, Fixed, Problematic, Manual };
        Status status = Status.All;
        public override void OnInspectorGUI() {
            if (EditorApplication.isCompiling)
                StopDownloadIssues();
            if (downloader != null && !downloader.IsPlaying())
                downloader = null;

            GUI.enabled = !EditorApplication.isCompiling && !EditorApplication.isPlaying;
            #region Toolbar 1
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("Alias", EditorStyles.miniLabel);
            string _alias = EditorGUILayout.TextField(alias.String, EditorStyles.textField, GUILayout.Width(120));
            if (_alias != alias.String)
                alias.String = _alias;

            GUILayout.FlexibleSpace();

            if (!update_checked && GUILayout.Button("Check update", EditorStyles.toolbarButton, GUILayout.Width(90)))
                EditorCoroutine.start(DownloadUpdate());

            if (!Berry.Contact.Contact.IsSending() && GUILayout.Button("Bug Report", EditorStyles.toolbarButton, GUILayout.Width(90))) {
                new PrefVariable("ContactForm_AppealType").Int = (int) ContactForm.AppealType.BugReport;
                BerryPanel panel = BerryPanel.CreateBerryPanel();
                panel.editor = null;
                panel.editorTitle = "Contact";
            }

            EditorGUILayout.EndHorizontal();
            #endregion

            #region Info
            GUILayout.Label(string.Format("Project: {0} v{1} ({2}), Fixer Version: v{3}",
                project,
                project_version.ToString("F2"),
                sub,
                fixer_version.ToString("F2")),
                EditorStyles.centeredGreyMiniLabel);
            #endregion
            
            #region Update
            if (update_checked && new_fixer_version > 0) {
                if (new_fixer_version > fixer_version) {
                    EditorGUILayout.HelpBox("Fixer v" + new_fixer_version.ToString("F2") + " is avaliable to install.", MessageType.Warning);
                    if (GUILayout.Button("Install", EditorStyles.miniButton, GUILayout.Width(60)))
                        InstallUpdate();
                } else
                    EditorGUILayout.HelpBox("Last version of Fixer is installed.", MessageType.Info);
            }
            #endregion

            #region Toolbar 2
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            status = (Status) EditorGUILayout.EnumPopup(status, EditorStyles.toolbarPopup, GUILayout.Width(100));

            selected_type = EditorGUILayout.Popup(selected_type, issueTypes.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(100));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                DownloadIssues();

            if (GUILayout.Button("Check all", EditorStyles.toolbarButton, GUILayout.Width(73)))
                foreach (List<Issue> list in Issue.all.Values)
                    foreach (Issue issue in list)
                        if (!issue.isChecked && issue.errors.Count == 0)
                            issue.Check();

            EditorGUILayout.EndHorizontal();
            #endregion
        


            if (downloader != null)
                GUILayout.Label("Downloading...", EditorStyles.centeredGreyMiniLabel);
            if (EditorApplication.isCompiling)
                GUILayout.Label("Compiling...", EditorStyles.centeredGreyMiniLabel);
            if (EditorApplication.isPlaying)
                GUILayout.Label("Playing...", EditorStyles.centeredGreyMiniLabel);

            bool visible;
            foreach (List<Issue> list in Issue.all.Values)
                foreach (Issue issue in list) {
                    visible = false;
                    if (issueTypes.Count > 0 && issueTypes[selected_type] != issue.type && issueTypes[selected_type] != "All Types")
                        continue;
                    switch (status) {
                        case Status.All:
                            visible = true;
                            break;
                        case Status.Checked:
                            visible = issue.isChecked && !issue.isFixed;
                            break;
                        case Status.Fixed:
                            visible = issue.isFixed;
                            break;
                        case Status.Manual:
                            visible = issue.fixer == "" && issue.codefixes.Count == 0;
                            break;
                        case Status.Problematic:
                            visible = issue.errors.Count > 0;
                            break;
                        case Status.Unchecked:
                            visible = !issue.isChecked;
                            break;
                    }
                    if (!visible)
                        continue;
                    DrawCard(issue);
                }

            GUI.enabled = true;
        }

        void DrawCard(Issue issue) {
            Color color = GUI.backgroundColor;
            if (issue.isChecked || issue.isCached)
                GUI.backgroundColor = Color.Lerp(color, (issue.isFixed || issue.isCached)? Color.green : Color.yellow, 0.3f);
            if (issue.errors.Count > 0)
                GUI.backgroundColor = Color.Lerp(color, Color.red, 0.3f);
            EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.ExpandWidth(true));
            GUI.backgroundColor = color;

            if (!string.IsNullOrEmpty(issue.alias))
                GUILayout.Label(string.Format("Alias: {0}", issue.alias), EditorStyles.boldLabel);
            GUILayout.Label(string.Format("Issue #{0} ({1})", issue.number, issue.type), EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(issue.desription, MessageType.None);

            List<string> messages = new List<string>();
            if (issue.fixer != "" || issue.codefixes.Count > 0) {
                bool isLocked = !GUI.enabled;
                if (!isLocked) {
                    messages.AddRange(issue.GetProblems());
                    if (messages.Count > 0)
                        GUI.enabled = false;
                }
                if (!issue.isChecked) {
                    if (GUILayout.Button("Check it", EditorStyles.miniButton, GUILayout.Width(80))) {
                        issue.Check();
                    }
                } else if (!issue.isFixed) {
                    messages.Add("Unfixed");
                    if (GUILayout.Button("Fix it", EditorStyles.miniButton, GUILayout.Width(80))) {
                        issue.Fix();
                    }
                } else
                    messages.Add("Fixed");

                if (issue.isCached && !issue.isFixed)
                    messages.Add("Fixed (Cache)");
                if (messages.Count > 0)
                    GUILayout.Label(string.Join("\n", messages.ToArray()), EditorStyles.boldLabel);

                GUI.enabled = !isLocked;
            }

            foreach (string error in issue.errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);

            if (issue.filesReport.Count > 0) {
                if (GUILayout.Button("Report it", EditorStyles.miniButton, GUILayout.Width(80))) {
                    new PrefVariable("ContactForm_Attachments").String = string.Join(";", issue.filesReport.ToArray());
                    new PrefVariable("ContactForm_AppealType").Int = (int) ContactForm.AppealType.BugReport;
                    new PrefVariable("ContactForm_Subject").String = "Problem with Fixer";
                    new PrefVariable("ContactForm_Body").String = string.Format("Project: {0} v{1} ({2})\nFixer v{3}\nIssue #{4} ({5})\n{6}", project, project_version.ToString("F2"), sub, fixer_version.ToString("F2"), issue.number, issue.type, issue.desription);
                    new PrefVariable("ContactForm_Log").String = string.Join("\n\n", issue.errors.ToArray());
                    BerryPanel panel = BerryPanel.CreateBerryPanel();
                    panel.editor = null;
                    panel.editorTitle = "Contact";
                }
            }

            EditorGUILayout.EndVertical();
        }

        public class Issue {

            public static Dictionary<string, List<Issue>> all = new Dictionary<string, List<Issue>>();

            public string fixer = "";
            public string checker = "";
            public List<string> namespaces = new List<string>() { "UnityEngine", "System.Collections", "System.Linq" };

            public List<Codefix> codefixes = new List<Codefix>();
            public List<string> errors = new List<string>();

            public string desription = "";
            public string type = "Other";
            public string alias = "";
            public int cacheCode = 0;
            public int number = 0;

            public bool isChecked = false;
            public bool isFixed = false;
            public bool isCached = false;
            public bool isExecutable {
                get {
                    return fixer != "" || checker != "";
                }
            }

            public float minVersion = 0;
            public List<string> subproject = new List<string>() { Fixer.sub };
            public List<int> required = new List<int>();

            public List<string> GetProblems() {
                List<string> issues = new List<string>();
                if (minVersion > fixer_version)
                    issues.Add("Required Fixer v" + minVersion.ToString("F2") + " or higher");
                if (required.Count > 0) {
                    int[] unfixed = required.Where(x => all[alias].Count <= x || !all[alias][x].isFixed).ToArray();
                    if (unfixed.Length > 0) {
                        foreach (int c in unfixed)
                            issues.Add("Required to fix Issue #" + c);
                    }
                }
                if (errors.Count > 0)
                    issues.Add("It can't be executed while it has any errors");

                return issues;
            }

            string id;

            public Issue() {
                id = Mathf.Abs(GetHashCode()).ToString();
            }

            public void Initialize() {
                if (codefixes.Count > 0) {
                    List<FileInfo> fileInfos = SearchFiles(dataPath).Where(x => x.Extension == ".cs").ToList();
                    foreach (Codefix codefix in codefixes) {
                        FileInfo fileInfo = fileInfos.Find(x => x.Name == codefix.filename + ".cs");
                        if (fileInfo != null) {
                            codefix.filepath = fileInfo.FullName;
                            codefix.original = CodeToRegularExpression(codefix.original);
                        } else {
                            errors.Add("\"" + codefix.filename + "\" file is not found");
                            CacheIt(false);
                        }
                    }
                }
            }

            EvaluatorFixer evaluator;
            Core core;

            void Compile() {
                if (!isExecutable || core != null)
                    return;
                string source;
                checker = checker.Trim();
                fixer = fixer.Trim();

                source = string.Join("", namespaces.Select(x => "using " + x + ";\n").ToArray());

                source += @"
                    namespace LocalFixer" + id + @" {
                        public class Code {    
                            public bool Check() {
                                bool result = true;
                                " + checker + @"
                                return result;
                            }

                            public void Fix() {
                                " + fixer + @"
                            }
                        }
                    }
                ";

                if (evaluator == null) {
                    CompilerSettingsFixer settings = new CompilerSettingsFixer();

                    MemoryStream stream = new MemoryStream();
                    StreamWriter writter = new StreamWriter(stream);
                    StreamReportPrinterFixer report = new StreamReportPrinterFixer(writter);

                    CompilerContextFixer context = new CompilerContextFixer(settings, report);
                    evaluator = new EvaluatorFixer(context);

                    foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                        evaluator.ReferenceAssembly(assembly);
                }
                
                object _core = null;
                try {
                    evaluator.Compile(source);
                    bool result_set = false;
                    evaluator.Evaluate("new LocalFixer" + id + ".Code()", out _core, out result_set);
                    if (!result_set)
                        return;
                    core = new Core(_core);
                } catch (System.Exception e) {
                    errors.Add(e.Message + "\n" + e.StackTrace);
                    CacheIt(false);
                    return;
                }
            }

            public void Check() {
                Compile();
                if (!EditorApplication.isCompiling && (core != null || !isExecutable) && errors.Count == 0) {
                    if (GetProblems().Count > 0) {
                        CacheIt(false);
                        return;
                    }
                    
                    int fixedCode = 0;
                    if (codefixes.Count > 0) {
                        foreach (Codefix codefix in codefixes) {
                            StreamReader fileR = new StreamReader(codefix.filepath);
                            string soucre = fileR.ReadToEnd();
                            fileR.Close();

                            Regex re = new Regex(CodeToRegularExpression(codefix.replacement), RegexOptions.Multiline);
                            MatchCollection matches = re.Matches(CodeWithoutComments(soucre));
                            if (matches.Count > 0)
                                fixedCode++;
                        }
                    }
                    
                    if (isExecutable) {
                        try {
                            isFixed = !core.Check() && fixedCode == codefixes.Count;
                        } catch (System.Exception e) {
                            errors.Add(e.Message + "\n" + e.StackTrace);
                            CacheIt(false);
                            return;
                        }
                    } else
                        isFixed = fixedCode == codefixes.Count;

                    CacheIt(isFixed);
                    isChecked = true;
                }
            }

            public List<string> filesReport = new List<string>();
            public void Fix() {
                Compile();
                if (!EditorApplication.isCompiling && (core != null || !isExecutable) && isChecked && !isFixed && errors.Count == 0) {
                    if (GetProblems().Count > 0) {
                        CacheIt(false);
                        return;
                    }

                    bool codeChanged = false;
                    filesReport.Clear();
                    Regex re;
                    foreach (Codefix codefix in codefixes) {
                        StreamReader fileR = new StreamReader(codefix.filepath);
                        string soucre = fileR.ReadToEnd();

                        re = new Regex(CodeToRegularExpression(codefix.replacement), RegexOptions.Multiline);
                        if (re.IsMatch(soucre))
                            continue;

                        re = new Regex(codefix.original, RegexOptions.Multiline);
                        MatchCollection matches = re.Matches(CodeWithoutComments(soucre));
                        if (matches.Count == 0) {
                            errors.Add("It is not possible to fix \"" + codefix.filename + "\" file.");
                            filesReport.Add(codefix.filepath);
                        }
                        if (errors.Count > 0)
                            continue;
                        foreach (Match match in matches) {
                            soucre = soucre.Remove(match.Index, match.Length);
                            soucre = soucre.Insert(match.Index, codefix.replacement);
                            codeChanged = true;
                        }
                        fileR.Close();

                        StreamWriter fileW = new StreamWriter(codefix.filepath);
                        fileW.WriteLine(soucre);
                        fileW.Close();
                    }

                    if (errors.Count > 0) {
                        CacheIt(false);
                        return;
                    }

                    if (isExecutable) {
                        try {
                            if (core.Check())
                                core.Fix();
                        } catch (System.Exception e) {
                            errors.Add(e.Message + "\n" + e.StackTrace);
                            CacheIt(false);
                            return;
                        }
                    }

                    isFixed = true;
                    CacheIt(isFixed);

                    if (codeChanged)
                        AssetDatabase.Refresh();
                }
            }

            void CacheIt(bool v) {
                List<int> _cache = new List<int>();
                if (!string.IsNullOrEmpty(cache.String))
                    _cache = cache.String.Split(';').Select(x => int.Parse(x)).ToList();
                if (v && !_cache.Contains(cacheCode))
                    _cache.Add(cacheCode);
                if (!v && _cache.Contains(cacheCode))
                    _cache.Remove(cacheCode);
                isCached = v;
                cache.String = string.Join(";", _cache.Select(x => x.ToString()).ToArray());
            }

            string CodeWithoutComments(string code) {
                Regex re = new Regex(@"(?://.*$|/\*.*\*/)", RegexOptions.Multiline);
                foreach (Match match in re.Matches(code))
                    code = code.Replace(match.Value, new string(' ', match.Value.Length));
                return code;
            }

            static string CodeToRegularExpression(string code) {
                string special_chars = @"\[]^.{}?+*|()$" + "\"";
                code = code.Trim();
                foreach (char c in special_chars)
                    code = code.Replace(c.ToString(), @"~\" + c.ToString() + @"~");
                code = code.Replace("~", @"\s*");

                Regex re = new Regex(@"\s+");
                code = re.Replace(code, @"\s+");

                return code;
            }

            public class Codefix {
                public string filename = "";
                public string filepath = "";
                public string original = "";
                public string replacement = "";
            }
            class Core {
                object core;
                public Core(object _core) {
                    if (_core == null)
                        throw new System.Exception("Core can't be null");
                    core = _core;
                }
                public bool Check() {
                    return (bool) core.GetType().GetMethod("Check").Invoke(core, null);
                }
                public void Fix() {
                    core.GetType().GetMethod("Fix").Invoke(core, null);
                }
            }
        }

        public override Object FindTarget() {
            return null;
        }
    }
}


