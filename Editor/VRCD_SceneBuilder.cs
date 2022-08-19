using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public class VRCD_SceneBuilder
{
    [MenuItem("VRCD/Build Scene Bundle")]
	public static string BuildSceneBundle(){ // Returns GUID for bundle
		Scene scene = EditorSceneManager.GetActiveScene();
		
		AssetBundleBuild build = new AssetBundleBuild();
		build.assetBundleName = scene.name + ".xworld";
		
		//build.assetNames = AssetDatabase.GetDependencies(scene.path);
		//build.assetNames = new string[dependencies.Length+1];
		//Array.Copy(dependencies, build.assetNames, dependencies.Length);
		//build.assetNames[dependencies.Length] = scene.path;
		build.assetNames = new string[]{ scene.path };
		
		if( !AssetDatabase.IsValidFolder("Assets/_VRCDSceneBundles") ){
			AssetDatabase.CreateFolder("Assets", "_VRCDSceneBundles");
		}
		string outputPath = AssetDatabase.GUIDToAssetPath( AssetDatabase.CreateFolder("Assets/_VRCDSceneBundles", scene.name) );
		
		Debug.Log("Building to " + outputPath + "...");
		AssetBundleManifest mani = BuildPipeline.BuildAssetBundles(
			outputPath,
			new AssetBundleBuild[] { build },
			BuildAssetBundleOptions.None,
			BuildTarget.StandaloneWindows
		);
		
		string[] bundles = mani.GetAllAssetBundles();
		if( bundles.Length < 1 ){
			Debug.Log("Error: Builder generated 0 bundles.");
			return "";
		}
		
		string guid = AssetDatabase.AssetPathToGUID( outputPath + "/" + bundles[0] );
		if( guid == "" ){
			Debug.Log("Error: Could not find generated bundle. Atypical characters in scene name?");
			return "";
		}
		
		return guid;
	}
}
