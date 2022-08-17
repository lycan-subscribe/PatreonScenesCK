using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class VRCD_BuildWindow : EditorWindow
{
	const string PATREON_CLIENT_ID = "5hPRg02JjlsKrIqWSQvbG-hvvUXrgJTXnvk3Fbzi8_Xn4-KHR3JGAytS_eR-0VY0";
	const string PATREON_AUTH_URL = "https://patreon.com/oauth2/authorize";
	const string PATREON_CREATOR_SCOPES = "identity campaigns campaigns.members";
	const string API_BASE_URL = "https://fa110miy6j.execute-api.us-west-2.amazonaws.com/patreon_vr_dev";
	
	new public Vector2 minSize = new Vector2(200,300);
	
	
	string _apiToken;
	
	private bool _loggedIn = false;
	private bool _isLoggingIn = false;
	private bool _hasTriedLogin = false;
	private UnityWebRequest _loginRequest = null;
	
	// Post login
	private PatreonResponse.Root _response;
	private int _campaignIndex = 0;
	private int _tierIndex = 0;
	
	[MenuItem("VRCD/Build Scene", false, 200)]
	static void Init()
	{
		VRCD_BuildWindow window = (VRCD_BuildWindow) GetWindow(typeof(VRCD_BuildWindow), false, $"Scene Builder");
		window.Show();
	}
	
	void Update(){
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
	}
	
	void OnGUI() {
		
		if(_loggedIn){
			Loggedin_Tab();
		}
		else{
			Login_Tab();
		}
		
	}
	
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
	
	private void Loggedin_Tab() {
		GUILayout.Label("Logged in.");
		
		// Warnings / errors in scene
		// ...
		
		_campaignIndex = EditorGUILayout.Popup(_campaignIndex, _response._campaign_name_list);
		_tierIndex = EditorGUILayout.Popup(_tierIndex, _response.getCampaign(_campaignIndex)._tier_name_list);
		
		if( GUILayout.Button("Build") ){
			VRCD_SceneBuilder.BuildSceneBundle();
		}
		
		if( GUILayout.Button("Build and Upload") ){
			string guid = VRCD_SceneBuilder.BuildSceneBundle();
			Debug.Log( "Generated " + AssetDatabase.GUIDToAssetPath( guid ) + ", uploading..." );
			UploadBundle(guid);
		}
		
	}
	
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
	
	private void Login() {
		if( _isLoggingIn ) return;
		
		var param = new Dictionary<string,string>{
			{ "access_token", _apiToken }
		};
		
		_loginRequest = UnityWebRequest.Get(
			API_BASE_URL + "/get_creator_data" + "?" + string.Join("&", param.Select(x => x.Key + "=" + x.Value).ToArray())
		);
		_loginRequest.SendWebRequest();
		_isLoggingIn = true;
		_hasTriedLogin = true;
	}
	
	private void UploadBundle(string guid){
		
	}
}