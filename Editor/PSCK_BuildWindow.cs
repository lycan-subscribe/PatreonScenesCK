using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows;

[InitializeOnLoad]
public class PSCK_BuildWindow : EditorWindow
{
	const string PATREON_CLIENT_ID = "5hPRg02JjlsKrIqWSQvbG-hvvUXrgJTXnvk3Fbzi8_Xn4-KHR3JGAytS_eR-0VY0";
	const string PATREON_AUTH_URL = "https://patreon.com/oauth2/authorize";
	const string PATREON_CREATOR_SCOPES = "identity campaigns campaigns.members";
	const string API_BASE_URL = "https://8ck36yi1oj.execute-api.us-west-2.amazonaws.com/patreon_vr_dev";
	
	new public Vector2 minSize = new Vector2(200,300);
	
	
	string _apiToken;
	
	private UnityWebRequest _loginRequest = null;
	private bool _loggedIn;
	private bool _isLoggingIn;
	private bool _hasTriedLogin;
	
	// Post login
	private PatreonResponse.Root _response;
	private int _campaignIndex;
	private int _tierIndex;
	private bool _tierRequirement;
	
	private UnityWebRequest _uploadRequest = null;
	private byte[] _bundleToUpload;
	private bool _isGettingUploadLink;
	private bool _isUploading;
	private bool _hasTriedUpload;
	private bool _uploadSucceeded;
	private string _lastUploadedURL;
	
	[MenuItem("PSCK/Build Scene", false, 200)]
	static void Init()
	{
		PSCK_BuildWindow window = (PSCK_BuildWindow) GetWindow(typeof(PSCK_BuildWindow), false, $"Scene Builder");
		window.Show();
	}
	
	void OnEnable(){
		EditorApplication.update -= EditorUpdate;
        EditorApplication.update += EditorUpdate;
		
		// Log in automatically
	}
	
	void OnDisable(){
		EditorApplication.update -= EditorUpdate;
	}
	
	private void EditorUpdate(){
		if( _loginRequest != null ){
			if( _isLoggingIn && _loginRequest.isDone ){
				_isLoggingIn = false;
				
				if( _loginRequest.isHttpError || _loginRequest.isNetworkError ){
					_hasTriedLogin = true;
				}
				else{
					_response = PatreonResponse.Root.CreateFromJSON(_loginRequest.downloadHandler.text);
					_loggedIn = true;
				}
			}
		}
		
		if( _uploadRequest != null ){
			if( _isUploading && _uploadRequest.isDone ){
				_isUploading = false;
				
				_uploadSucceeded = !_uploadRequest.isHttpError && !_uploadRequest.isNetworkError;
				
				if( !_uploadSucceeded ){
					Debug.Log( _uploadRequest.downloadHandler.text );
				}
			}
			
			else if( _isGettingUploadLink && _uploadRequest.isDone ){
				_isGettingUploadLink = false;
				
				if( _uploadRequest.isHttpError || _uploadRequest.isNetworkError ){
					_uploadSucceeded = false;
					return;
				}
				
				string target_url = UploadResponse.GetURLFromJSON( _uploadRequest.downloadHandler.text );
				Debug.Log( "Uploading to " + target_url + " ..." );
				
				UploadBundleToS3( target_url );
			}
		}
	}
	
	// Runs around 50 times per second, refreshes the entire GUI
	void OnGUI() {
		
		if(_loggedIn){
			Loggedin_Tab();
		}
		else{
			Login_Tab();
		}
		
	}
	
	// The tab before an access token is given
	private void Login_Tab() {
		if( GUILayout.Button("Generate Token") ){
			GetToken();
		}
		
		_apiToken = EditorGUILayout.TextField("API Token", _apiToken);
		
		if( GUILayout.Button("Log in") ){
			Login();
		}
		
		if( !_isLoggingIn && _hasTriedLogin ){
			GUILayout.Label("Failed to login. Is your token expired? Is the SDK outdated?");
			GUILayout.Label("DEBUG: " + _loginRequest.error);
		}
		else if( _isLoggingIn ){
			GUILayout.Label("Logging in...");
		}
	}
	
	// The tab after an access token is given and we put patreon data in _response
	private void Loggedin_Tab() {
		GUILayout.Label("Logged in.");
		
		// Warnings / errors in scene
		// ...
		
		_campaignIndex = EditorGUILayout.Popup(_campaignIndex, _response._campaign_name_list);
		_tierIndex = EditorGUILayout.Popup(_tierIndex, _response.getCampaign(_campaignIndex)._tier_name_list);
		
		_tierRequirement = EditorGUILayout.Toggle("Require tier membership", _tierRequirement);
		
		if( GUILayout.Button("Build") ){
			PSCK_SceneBuilder.BuildSceneBundle();
		}
		
		if( GUILayout.Button("Build and Upload") ){
			string guid = PSCK_SceneBuilder.BuildSceneBundle();
			Debug.Log( "Generated " + AssetDatabase.GUIDToAssetPath( guid ) + ", uploading..." );
			UploadBundle(guid);
		}
		
		if( _isGettingUploadLink || _isUploading ){
			GUILayout.Label("Uploading ...");
		}
		
		if( _hasTriedUpload && !_isGettingUploadLink && !_isUploading ){
			if( _uploadSucceeded ){
				GUILayout.Label("Upload succeeded. Public link:");
				EditorGUILayout.SelectableLabel(_lastUploadedURL);
			}
			else{
				GUILayout.Label("Error uploading: " + _uploadRequest.error);
			}
		}
		
	}
	
	// Call this to open a web browser that will get the user a new token
	private void GetToken() {
		var param = new Dictionary<string,string>{
			{ "response_type", "code" },
			{ "scope", UnityWebRequest.EscapeURL( PATREON_CREATOR_SCOPES ) },
			{ "client_id", PATREON_CLIENT_ID },
			{ "redirect_uri", UnityWebRequest.EscapeURL(API_BASE_URL + "/get_upload_token") }
		};
		
		string url = PATREON_AUTH_URL + "?" + string.Join("&", param.Select(x => x.Key + "=" + x.Value).ToArray());
		
		Application.OpenURL(url);
	}
	
	// Call this to start the web request that will return the user's patreon data if the token is valid
	// This web request will be done after _isLoggingIn is true but _loginRequest.isDone is also true
	private void Login() {
		if( _isLoggingIn ) return;
		
		_loginRequest = UnityWebRequest.Get(
			API_BASE_URL + "/get_creator_data"
		);
		
		byte[] token_bytes = Encoding.UTF8.GetBytes(_apiToken);
		_loginRequest.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(token_bytes));
		
		_loginRequest.SendWebRequest();
		_isLoggingIn = true;
		_hasTriedLogin = true;
	}
	
	// Call this to start the web request that will get an upload link from the API to put the bundle on S3
	// This web request will be done after _isGettingUploadLink is true but _uploadRequest.isDone is also true, when the upload should actually start
	// The actual upload should be done when _isUploading is true but _uploadRequest.isDone is also true, the same web request will be reused
	private void UploadBundle(string guid){
		if( _isUploading || _isGettingUploadLink ) return;
		
		string asset_path = AssetDatabase.GUIDToAssetPath(guid);
		_bundleToUpload = File.ReadAllBytes( asset_path );
		
		PatreonResponse.Campaign target_campaign = _response.getCampaign( _campaignIndex );
		string required_tier = target_campaign.getTierID( _tierIndex );
		if( !_tierRequirement ){
			required_tier = "None";
		}
		
		var param = new Dictionary<string,string>{
			{ "content_length", _bundleToUpload.Length.ToString() },
			{ "file_name", asset_path.Split('/').Last() },
			{ "patreon_campaign", target_campaign.id },
			{ "patreon_required_tier", required_tier }
		};
		
		_uploadRequest = UnityWebRequest.Get(
			API_BASE_URL + "/upload_bundle" + "?" +
			string.Join("&", param.Select(x => x.Key + "=" + x.Value).ToArray())
		);
		
		byte[] token_bytes = Encoding.UTF8.GetBytes(_apiToken);
		_uploadRequest.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(token_bytes));
		
		_uploadRequest.SendWebRequest();
		_isGettingUploadLink = true;
		_hasTriedUpload = true;
		
		
		// Save the target URL
		
		var target_param = new Dictionary<string,string>{
			{ "response_type", "code" },
			{ "scope", UnityWebRequest.EscapeURL( PATREON_CREATOR_SCOPES ) },
			{ "client_id", PATREON_CLIENT_ID },
			{ "redirect_uri", UnityWebRequest.EscapeURL(API_BASE_URL + "/get_world_patreon_gate") },
			{ "state", _response.getUserID() + "-" + asset_path.Split('/').Last() }
		};
		
		_lastUploadedURL = PATREON_AUTH_URL + "?" + string.Join("&", target_param.Select(x => x.Key + "=" + x.Value).ToArray());
	}
	
	private void UploadBundleToS3(string target_url){
		_uploadRequest = UnityWebRequest.Put(target_url, _bundleToUpload);
		_uploadRequest.SetRequestHeader("Content-Type", "application/x-world");
		
		PatreonResponse.Campaign target_campaign = _response.getCampaign( _campaignIndex );
		string required_tier = target_campaign.getTierID( _tierIndex );
		if( !_tierRequirement ){
			required_tier = "None";
		}
		_uploadRequest.SetRequestHeader("x-amz-tagging",
			"patreon_id=" + _response.getUserID()
			+ "&patreon_campaign=" + target_campaign.id
			+ "&patreon_required_tier=" + required_tier
		);
		
		_uploadRequest.SendWebRequest();
		_isUploading = true;
	}
}