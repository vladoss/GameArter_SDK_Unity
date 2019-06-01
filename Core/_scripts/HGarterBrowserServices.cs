using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using GameArter.Ads;

[HideInInspector]
public class HGarterBrowserServices {
#if UNITY_WEBGL
	// javascript browser function initialization

	// init info
	[DllImport("__Internal")]
	private static extern void _gb_initGameSignature(string info);
	public void InitGameSignature(string sdkVersion, char interpreter, uint gameId, bool multiplayer, byte projectVersion){
		SdkInitInfo info = new SdkInitInfo ();
		info.e = "unity";
		info.ev = Application.unityVersion;
		info.s = sdkVersion;
		info.i = interpreter;
		info.g = gameId;
		info.p = Application.platform.ToString().ToLower();
		info.m = multiplayer;  // multiplayer game - useless since ver 3.0
		info.pv = projectVersion;
		_gb_initGameSignature(JsonUtility.ToJson (info));
	}

	// Game initialized info
	[DllImport("__Internal")]
	private static extern void _gb_gameInitialized(string state);
	public void GameInitialized(string state){
		_gb_gameInitialized(state);
	}
		
	// Ad requests
	[DllImport("__Internal")]
	private static extern void _gb_adRequest(string spData);
	public void AdRequest(AdUnitData adData, Action<string> callback)
    {
        if (adData.adType == AdsConfiguration.AdType.Fullscreen)
        {
            if (callback != null) Garter.I._AddCatchCbListener<string>(Garter._CachedListener._AdState, callback);
            if (!adData.editorMode)
            {
                _gb_adRequest(JsonUtility.ToJson(new SpData(2, adData.mutedGame, adData.cursorLockState, adData.visibleCursor)));
            }
            else
            {
                Debug.LogWarning("Ads work after uploading to GameArter and using GameArter gameplayer");
            }
        } else {
            Debug.Log("Only fullscreen ads are supported on web platform");
            if (callback != null) callback("unsupported");
        }
	}

	// Analytics request
	[DllImport("__Internal")]
	private static extern void _gb_analyticsRequest(string analyticsData);
	public void AnalyticsRequest(string mode, string scene, string category, string action, string label, int value){
        if (!Garter.I._IsEditorMode())
        {
            AnalyticsClass analytics = new AnalyticsClass();
            analytics.mode = mode;
            analytics.scene = scene;
            analytics.category = category;
            analytics.action = action;
            analytics.label = label;
            analytics.value = value;
            _gb_analyticsRequest(JsonUtility.ToJson(analytics));
        }
        else
        {
            Debug.Log("[GameArter][Analytics] (info msg) | Posting " + mode + " / " + scene + " / " + category + "/" + action + "/" + label + "/" + value + " to analytics. | NOTE: Feature works in GameArter gameplayer only.");
        }
	}

    public void OpenLeaderboardUI(string data){
        OpenModule(data);
    }

	// Open browser window (module)
	[DllImport("__Internal")]
	private static extern void _gb_openModuleWindow(string data);
	public void OpenModule(string data){
        _gb_openModuleWindow(data);
	}
		
	// Story animation
	[DllImport("__Internal")]
	private static extern void _gb_storyAnimation(int num);
	public void StoryAnimation(int num){
        if (!Garter.I._IsEditorMode())
        {
            _gb_storyAnimation(num);
        }
        else
        {
            Debug.Log("[GameArter] Cutscene " + num + " will be displayed after uploading to GameArter.");
        }
	}

	// Story animation
	[DllImport("__Internal")]
	private static extern void _gb_userBilanceCallback(string data);
	public void UserBilanceCallback(string data){
		_gb_userBilanceCallback(data);
	}

	// activity ping
	[DllImport("__Internal")]
	private static extern void _gb_activityPing(int activityState);
	public void ActivityPing(int activityState){
		_gb_activityPing(activityState);
	}


	// Get IndividualGameSettings
	[DllImport("__Internal")]
	private static extern void _gb_gameSettings(string data);
	public void GameSettings(string data){
		_gb_gameSettings(data);
	}

	// Game restart
	[DllImport("__Internal")]
	private static extern void _gb_gameRestart();
	public void GameRestart(){
        if (!Garter.I._IsEditorMode())
        {
            _gb_gameRestart();
        }
        else
        {
            Debug.Log("[GameArter] GameReload is supported after uploading to gamearter.");
        }
        
	}
		
	// Ban
	[DllImport("__Internal")]
	private static extern void _gb_gameBan(string data);
	public void GameBan(string data){
		_gb_gameBan(data);
	}

	// redirect
	[DllImport("__Internal")]
	private static extern void _gb_openWindow(string url);
	public void Redirect(string url, bool _blank){
		if (_blank && !Application.isEditor) {
			_gb_openWindow(url);
		} else {
			Application.OpenURL(url);
		}
	}

	// fullscreen
	[DllImport("__Internal")]
	private static extern void _gb_fullscreen();
	public void Fullscreen(){
		_gb_fullscreen ();
	}

	// Install as PWA
	[DllImport("__Internal")]
	private static extern void _gb_installAsPwa();
	public void InstallAsPWA(){
		_gb_installAsPwa ();
	}

	// Get time to next ad and min ad meantime
	[DllImport("__Internal")]
	private static extern void _gb_getAdConf();
	public void GetAdConfiguration(){
		_gb_getAdConf ();
	}

    public void Invite() { }

	// Identification for BrowserSDK
	[System.Serializable]
	internal class SdkInitInfo {
		public string e; // engine
		public string ev; // engine version
		public string s; // sdk version
		public char i; // interpreter
		public uint g; // gameId
		public string p; // platform
		public bool m; // multiplayer game
		public byte pv; // projectVersion
	}
		
	//wrapper for ad calls
	[System.Serializable]
	internal class SpData 
	{
		public int t; //type of adCall
		public bool m; //mutedGame
		public string f; //focusState
		public bool c; //cursor visibility state
		public SpData(int adCallNum, bool mutedGame, string focusState, bool visibleCursor){
			this.t = adCallNum;
			this.m = mutedGame;
			this.f = focusState;
			this.c = visibleCursor;
		}
	}

	// Analytics data wrapper
	[System.Serializable]
	internal class AnalyticsClass
	{
		public string mode;
		public string scene;
		public string category;
		public string action;
		public string label;
		public int value;
	}
#endif
}