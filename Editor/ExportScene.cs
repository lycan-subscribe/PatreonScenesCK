using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ExportScene
{
    [MenuItem("VRCD/Build Scene Bundle")]
	static void BuildSceneBundle(){
		Scene scene = EditorSceneManager.GetActiveScene();
		Debug.Log(scene.name);
		
		/*bool res = BuildPipeline.BuildAssetBundle(
			(Object) scene,
			new string[0],
			"Assets/SceneBundles/" + scene.name + ".xworld",
			BuildAssetBundleOptions.None,
			BuildTarget.StandaloneWindows
		);*/
		
		AssetBundleBuild build = new AssetBundleBuild();
		build.assetBundleName = scene.name + ".xworld";
		
		//build.assetNames = AssetDatabase.GetDependencies(scene.path);
		//build.assetNames = new string[dependencies.Length+1];
		//Array.Copy(dependencies, build.assetNames, dependencies.Length);
		//build.assetNames[dependencies.Length] = scene.path;
		build.assetNames = new string[]{ scene.path };
		
		bool res = BuildPipeline.BuildAssetBundles(
			"Assets/SceneBundles/",
			new AssetBundleBuild[] { build },
			BuildAssetBundleOptions.None,
			BuildTarget.StandaloneWindows
		);
	}
}
