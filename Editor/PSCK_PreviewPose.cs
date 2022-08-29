using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof(ViewerPose))]
public class PSCK_PreviewPose : Editor
{
	ViewerAvatar avatar = null;

	void OnEnable(){ // Pose selected
		EditorApplication.update += Update;

		/* This makes a clone in the client, pls find an editor method
		if( Application.isPlaying ){
			// See if an avatar exists
			ViewerAvatar current_avi = FindObjectOfType<ViewerAvatar>();
			if( current_avi != null ){
				// Make a copy
				avatar = Instantiate(current_avi);

				// Set the animator
				avatar.SetController( ((ViewerPose) target).pose_controller );
			}
		}
		*/
	}

	void OnDisable(){ // Something else selected
		EditorApplication.update -= Update;

		if(avatar != null){
			DestroyImmediate(avatar.gameObject);
			avatar = null;
		}
	}
	
	void Update(){
		if(avatar != null){
			avatar.transform.position = ((ViewerPose) target).transform.position;
			avatar.transform.rotation = ((ViewerPose) target).transform.rotation;
			avatar.transform.localScale = ((ViewerPose) target).transform.localScale;
		}
	}
}