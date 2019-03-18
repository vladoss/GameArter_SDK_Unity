using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using UnityEngine.UI;

public class StaticHelpersGarterSDK : MonoBehaviour {
	/***************************** debugger ***************************/
	public static void SdkDebugger(string request, string response, string note = "-"){
		GameObject[] sdkDebugger = new GameObject[3]{GameObject.Find("Debugger/Garter_Req/Request"), GameObject.Find("Debugger/Garter_Res/Response"), GameObject.Find("Debugger/Garter_Note/Note")};
		sdkDebugger [0].GetComponent<Text> ().text = request;
		sdkDebugger [1].GetComponent<Text> ().text = response;
		sdkDebugger [2].GetComponent<Text> ().text = note;
	}

	/***************************** General Features ***************************/
	public void CallAd(){
		Garter.I.CallAd (2);
		SdkDebugger("Garter.I.CallAd (2)", "-", "If no ad displayed, there was not enough time from prev ad");
	}
	public void CallAdWithCb(){
		SdkDebugger("CallAd (2, (state) => {})", "-", "returns state of ad in a callback");
		Garter.I.CallAd (2, (state) => {
			SdkDebugger("CallAd (2, (state) => {})", state.ToString(), "returns state of ad in a callback");
			if(state == "loaded"){
				// MUTE GAME
			} else if(state == "completed"){
				// UNMUTE GAME
			} else {
				// ...
			}
		});
	}

	public void GetAdConf(){
		Garter.I.GetAdConf ((conf) => {
			SdkDebugger("Garter.I.GetAdConf ((conf) => {})", ("Next ad in "+conf.nextAdM.ToString()+"s. Minimum time between ads "+conf.meantimeM.ToString()+"s."), "returns ads configuration.");
		});
	}

	public void RewardAd(){
		Garter.I.RewardedAd();
		SdkDebugger("Garter.I.RewardedAd()", "-", "-");
	}
	public void SendToAnalytics(){
		Garter.I.AnalyticsEvent ("item-used","whatItem","fromWhere");
		SdkDebugger("Garter.I.ToAnalytics (\"item-used\", \"item1\", \"stack\")","-","User used item1 from his stack");
	}
	public void IndividualGameMode(){
		byte limitSomething = Garter.I.IndividualGameMode ();
		SdkDebugger("Garter.I.IndividualGameMode ()", limitSomething.ToString() , "-");
	}

	public void OpenModule(string moduleNameId){
		Garter.I.OpenSdkWindow (moduleNameId);
		SdkDebugger("Garter.I.OpenSdkWindow ("+moduleNameId+");","-","Open module request");
	}

	public void InstallAsApp(){
		if(Garter.I.GetStatePWA() == "enabled"){
			Garter.I.InstallAsPWA<string> ((state) => {
				Debug.Log ("PWA installation status: " + state);
			});
		} else {
			Debug.Log("PWA service is disabled");	
		}
	}

	/***************************** events ***************************/
	public void AddKills(int increaseBy){
		decimal value = Garter.I.Event ("kills", increaseBy);
		SdkDebugger("Garter.I.Event (\"kills\", "+increaseBy+")", value.ToString(), "all data (for illustration): kills: " + value + " | deaths: " + Garter.I.Event ("deaths"));
	}
	public void GetKills(){
		decimal value = Garter.I.Event ("kills");
		SdkDebugger("Garter.I.Event (\"kills\")", value.ToString(), "all data (for illustration): kills: " + value + " | deaths: " + Garter.I.Event ("deaths"));
	}

	public void AddDeaths(int increaseBy){
		decimal value = Garter.I.Event ("deaths", increaseBy);
		SdkDebugger("Garter.I.Event (\"kills\", "+increaseBy+")", value.ToString(), "all data (for illustration): kills: " + value + " | deaths: " + Garter.I.Event ("deaths"));
	}

	public void TakeTime(){
		decimal value = Garter.I.Event ("bestTime", 0.32M);
		Debug.Log (value);
		SdkDebugger("Garter.I.Event (\"bestTime\", 0.32M)", value.ToString("0.000"));
	}
	public void GetTime(){
		decimal value = Garter.I.Event ("bestTime");
		SdkDebugger("Garter.I.Event (\"bestTime\")", value.ToString("0.000"));
	}

	/************************** User data *************************************/

	public void UserData(string info){
		switch (info) {
		case "nick":
			SdkDebugger("Garter.I.UserNick()",Garter.I.UserNick(),"-");
			break;
		case "image":
			SdkDebugger("Garter.I.UserImage()","Texture2D image","Returns user's profile image in Texture2D format");
			break;
		case "lang":
			SdkDebugger("Garter.I.UserLang()",Garter.I.UserLang(),"User's prefered language");
			break;
		case "country":
			SdkDebugger("Garter.I.UserCountry()",Garter.I.UserCountry(),"Country a user is connected from");
			break;
		case "browser":
			SdkDebugger("Garter.I.GetBrowserName()",Garter.I.GetBrowserName(),"Browser a game is running in");
			break;
		};
	}

	public void GetPhotonId(){ //= AppId
		if (Garter.I.GetMultiplayerNetwork () != null) {
			string appId = Garter.I.GetMultiplayerNetwork () [0];
			SdkDebugger ("Garter.I.GetMultiplayerNetwork()[0]", appId, "Returns photon ID");
		} else {
			SdkDebugger ("-", "-", "Multiplayed is not set in init config");
		}
	}

	public void GetPhotonVer(){ //=AppVersion
		if (Garter.I.GetMultiplayerNetwork () != null) {
			string AppVersion = Garter.I.GetMultiplayerNetwork()[1];
			SdkDebugger("Garter.I.GetMultiplayerNetwork()[1]", AppVersion, "Returns photon id ver");
		} else {
			SdkDebugger ("-", "-", "Multiplayed is not set in init config");
		}
	}

	public void Redirect(bool toNewTab){
		Garter.I.OpenWebPage ("https://www.google.com", "individual", toNewTab);
		if (toNewTab) {
			SdkDebugger ("Garter.I.OpenWebPage (\"https://www.google.com\")", "-", "-");
		} else {
			SdkDebugger ("Garter.I.OpenWebPage (\"https://www.google.com\",\"individual\", false)", "-", "-");
		}
	}

	public void Fullscreen(){
		SdkDebugger("Garter.I.Fullcreen ()","-","Feature does not work in Unity editor");
		Garter.I.Fullcreen ();
	}


	/***************************** Alerts ***************************/
	public void SettingsAlert(bool display){
		Garter.I.SettingsAlert (display);
		SdkDebugger("Garter.I.SettingsAlert ("+display+")", "-", "-");
	}
	public void ShopAlert(bool display){
		Garter.I.ShopAlert (display);
		SdkDebugger("Garter.I.ShopAlert ("+display+")", "-", "-");
	}
		
	public void ClearUserData(bool directly){
		if (directly) {
			Garter.I.ClearDataUserConfirm ();
			SdkDebugger("Garter.I.ClearDataUserConfirm ()","-","Clear saved data request");
		} else {
			Garter.I.ClearDataUserReq ();
			SdkDebugger("Garter.I.ClearDataUserReq ()","-","Clear saved data request");
		}
	}

	public void RunStory(){
		SdkDebugger("Garter.I.RunStoryAnimation (0)","-","Run story number 0 (Feature does not work in editor)");
		Garter.I.RunStoryAnimation (0);
	}

	public void GameProgress(bool getProgress){
		if (getProgress) {
			SdkDebugger ("Garter.I.UserProgress ();", Garter.I.UserProgress ().ToString(), "-");
		} else { // set
			// generate random float number
			System.Random random = new System.Random ();
			float randomProgressVal = (float)random.Next(0, 100);
			SdkDebugger ("Garter.I.UserProgress ("+randomProgressVal+");", Garter.I.UserProgress (randomProgressVal).ToString(), "Depends on game progress counting option");
		}
	}

	public void GetBrowserActivityState(){
		SdkDebugger ("Garter.I.GetBrowserTabState ()", Garter.I.GetBrowserTabState (), "Browser tab state");
	}
		
	public Texture officeMap = null;

	int updateLimit = 1001;
	public void LoadAssetScreen(){
		SdkDebugger("Garter.I.CreateLoadingScreen (\"Loading map: Office\", officeMap, true)","-","-");
		Garter.I.CreateLoadingScreen ("Loading map: Office", officeMap, true);
		updateLimit = 0;
	}

	void Update(){
		updateLimit ++;
		if (updateLimit < 1000) {
			Garter.I.UpdateLoadingScreen (updateLimit / 10);
		} else if (updateLimit == 1000){
			Garter.I.RemoveLoadingScreen ();
		}

	}

	public void NextScene(){
		SceneManager.LoadScene (1);
	}

	public void LoadScene(int sceneIndex){
		SceneManager.LoadScene (sceneIndex);
	}
}
