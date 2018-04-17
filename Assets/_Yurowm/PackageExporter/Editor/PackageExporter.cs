using System.Linq;
using UnityEditor;
using UnityEngine;

public class PackageExporter : Editor {

    [MenuItem("Export/Export Package")]
    static void Export() {
        string[] projectContent = AssetDatabase.GetAllAssetPaths();
        projectContent = projectContent.Where(x => PassAsset(x)).ToArray();
        string _name = "ExportedAsset_" + System.DateTime.Now.ToString() + ".unitypackage";
        _name = _name.Replace('/', '-').Replace(' ', '_').Replace(':', '-');

        EditorUtility.DisplayProgressBar("Exporting", "Exporting the package", 0.3f);

        AssetDatabase.ExportPackage(projectContent, _name);

        EditorUtility.ClearProgressBar();

        EditorUtility.RevealInFinder(Application.dataPath);
    }

    static bool PassAsset(string path) {
        if (path.StartsWith("Assets/AssetStoreTools/")) return false;
        if (path.StartsWith("Assets/")) return true;
        if (path.StartsWith("ProjectSettings/")) return true;
        return false;
    }
}
