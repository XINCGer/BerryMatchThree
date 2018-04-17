#if UNITY_5
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

public class PostBuildProcessor : MonoBehaviour
{
	#if UNITY_CLOUD_BUILD
	public static void OnPostprocessBuildiOS (string exportPath)
	{
		Debug.Log("OnPostprocessBuildiOS");
		ProcessPostBuild(BuildTarget.iPhone,exportPath);
	}
	#endif

	[PostProcessBuild]
	public static void OnPostprocessBuild (BuildTarget buildTarget, string path)
	{
		#if !UNITY_CLOUD_BUILD
		Debug.Log ("OnPostprocessBuild");
		ProcessPostBuild (buildTarget, path);
		#endif
	}

	private static void ProcessPostBuild (BuildTarget buildTarget, string path)
	{
#if UNITY_IPHONE
        string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));

        string target = proj.TargetGuidByName("Unity-iPhone");
        //
        //Required Frameworks
        proj.AddFrameworkToProject(target, "AudioToolbox.framework", false);
        proj.AddFrameworkToProject(target, "AVFoundation.framework", false);
        proj.AddFrameworkToProject(target, "CoreGraphics.framework", false);
        proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);
        proj.AddFrameworkToProject(target, "CoreMedia.framework", false);
        proj.AddFrameworkToProject(target, "EventKit.framework", false);
        proj.AddFrameworkToProject(target, "EventKitUI.framework", false);
        proj.AddFrameworkToProject(target, "MediaPlayer.framework", false);
        proj.AddFrameworkToProject(target, "MessageUI.framework", false);
        proj.AddFrameworkToProject(target, "QuartzCore.framework", false);
        proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);

        proj.AddFileToBuild(target, proj.AddFile("usr/lib/libz.1.2.5.dylib", "Frameworks/libz.1.2.5.dylib", PBXSourceTree.Sdk));

        //Optional Frameworks
        proj.AddFrameworkToProject(target, "AdSupport.framework", true);
        proj.AddFrameworkToProject(target, "Social.framework", true);
        proj.AddFrameworkToProject(target, "StoreKit.framework", true);
        proj.AddFrameworkToProject(target, "Webkit.framework", true);

        File.WriteAllText(projPath, proj.WriteToString());
#endif
    }
}
#endif
