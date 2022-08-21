using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UploadResponse
{
	public string upload_url;
	
	public static string GetURLFromJSON(string jsonString){
		UploadResponse r = JsonUtility.FromJson<UploadResponse>(jsonString);
		
		return r.upload_url;
	}
}