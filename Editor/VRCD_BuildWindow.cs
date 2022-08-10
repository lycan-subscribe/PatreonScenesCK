using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class VRCD_BuildWindow : EditorWindow
{
	const string PATREON_CLIENT_ID = "5hPRg02JjlsKrIqWSQvbG-hvvUXrgJTXnvk3Fbzi8_Xn4-KHR3JGAytS_eR-0VY0";
	const string PATREON_AUTH_URL = "https://patreon.com/oauth2/authorize";
	const string API_BASE_URL = "https://fa110miy6j.execute-api.us-west-2.amazonaws.com/patreon_vr_dev";
	
	
	string _apiToken;
	
	private bool _loggedIn = false;
	private bool _isLoggingIn = false;
	private bool _hasTriedLogin = false;
	private UnityWebRequest _webRequest = null;
	
	[MenuItem("VRCD/Build Scene", false, 200)]
	static void Init()
	{
		VRCD_BuildWindow window = (VRCD_BuildWindow) GetWindow(typeof(VRCD_BuildWindow), false, $"Scene Builder");
		window.Show();
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
		
		EditorGUILayout.TextField("API Token", _apiToken);
		
		if( GUILayout.Button("Log in") ){
			Login();
		}
		
		if( !_isLoggingIn && _hasTriedLogin ){
			GUILayout.Label("Failed to login. Is your token expired? Is the SDK outdated?");
		}
	}
	
	private void Loggedin_Tab() {
		
	}
	
	private void GetToken() {
		var param = new Dictionary<string,string>{
			{ "response_type", "code" },
			{ "client_id", PATREON_CLIENT_ID },
			{ "redirect_uri", UnityWebRequest.EscapeURL(API_BASE_URL + "/get_upload_token") }
		};
		
		
		string url = PATREON_AUTH_URL + "?" + string.Join("&", param.Select(x => x.Key + "=" + x.Value).ToArray());
		
		Application.OpenURL(url);
	}
	
	private void Login() {
		if( _isLoggingIn ) return;
		
		var param = new Dictionary<string,string>{
			
		};
		
		_webRequest = UnityWebRequest.Get(
			API_BASE_URL + "?" + string.Join("&", param.Select(x => x.Key + "=" + x.Value).ToArray())
		);
		_webRequest.SendWebRequest();
		_isLoggingIn = true;
		_hasTriedLogin = true;
	}
}