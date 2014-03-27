using UnityEngine;
using System.Collections;
using KiiCorp.Cloud.Storage;
using System;
using KiiCorp.Cloud.Analytics;

public class AuthForm : MonoBehaviour {

	bool OnCallback = false;
	string username = "";
	string password = "";
	KiiUser user = null;

	// Use this for initialization
	void Start () {
	// KiiAutoInitialize and Editor based configuration only work when you test your game in Editor mode
	// For game deployment (eg. on Android/iOS) intialize like above via an early loading game object
		//Kii.Initialize("your_app_id", "your_app_key", Kii.Site.your_server_location);
		//KiiAnalytics.Initialize("your_app_id", "your_app_key", KiiAnalytics.Site.your_server_location, "unique_device_id");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI () {
		if (OnCallback)
			GUI.enabled = false;
		else
			GUI.enabled = true;
		
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		GUILayout.FlexibleSpace ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();

		GUI.contentColor = Color.blue;
		GUILayout.Label ("Username");
		username = GUILayout.TextField (username, GUILayout.MinWidth (200));
		GUILayout.Space (10);
		GUILayout.Label ("Password");
		password = GUILayout.PasswordField (password, '*', GUILayout.MinWidth (100));
		GUILayout.Space (30);
		
		if (GUILayout.Button ("Login", GUILayout.MinHeight (50), GUILayout.MinWidth (100))) {
			if( username.Length == 0 || password.Length == 0 )
				Debug.Log ("Username/password can't be empty");
			else {
				Login ();
			}
		}
		
		if (GUILayout.Button ("Register", GUILayout.MinHeight (50), GUILayout.MinWidth (100))) {
			if( username.Length == 0 || password.Length == 0 )
				Debug.Log ("Username/password can't be empty");
			else {
				Register ();
			}
		}
		
		if (user != null) {
			OnCallback = false;
			Application.LoadLevel ("Level");
		}

		GUILayout.EndVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndArea ();
	}

	private void Login () {
		user = null;
		OnCallback = true;
		KiiUser.LogIn(username, password, (KiiUser user2, Exception e) => {
			if (e == null) {
				Debug.Log ("Login completed");
				user = user2;
			} else {
				user = null;
				OnCallback = false;
				Debug.Log ("Login failed : " + e.ToString());
			}
		});
	}
	
	private void Register () {
		user = null;
		OnCallback = true;
		KiiUser built_user = KiiUser.BuilderWithName (username).Build ();
		built_user.Register(password, (KiiUser user2, Exception e) => {
			if (e == null)
			{
				user = user2;
				Debug.Log ("Register completed");
			} else {
				user = null;
				OnCallback = false;
				Debug.Log ("Register failed : " + e.ToString());
			}
			
		});
	}
}
