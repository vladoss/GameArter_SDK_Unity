// Copyright 2013 - 2018, Awagon Entertainment s.r.o., All rights reserved.
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

//data sending
using System.Text; //encoding
using System.Collections.Generic;

//advanced parsing
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GarterProtection;
using UnityEngine.UI;

public class Garter : MonoBehaviour {
	readonly string sdkVersion = "2.2";
	private static string serverAddress = "https://api.gamearter.com/game/sdk/1";  //PRODUCTION (cypher)
	//private static string serverAddress = "https://api.gamearter.com/test/sdk/1"; // DEV TESTNET

	private bool sdkInitialiized = false;
	private bool debugMode = false;

	private bool editorMode = false;
	private string referrer; // host domain info

	private bool itemSync = false;

	private bool firstLoad = true, activeSdk = false;
	private char interpreter = 'F';
	private byte projectVersion = 1;

	// user
	private Texture2D userPicture = null;//univerzalni foto
	private string[] user = new string[3]{"Guest", "en", "USA"}; // nick / language / country + userRank
	private byte userAuthentizationState = 0;// 0 = unknown / 1 = guest / 2 = logged

	private string[] multiplNetwork = null; //photonId, version (null = is not required / empty = is required)

	// connection counters
	private byte[] saveFrequency = new byte[2]{2, 6}; //frequency of saving values on server (2 mins for 6 requests)
	private Int32 lastSave = 0, lastTrackedManualSave = 0; //last save = sign, that we are working with online data
	private byte manualRequestsCounter = 0;

	//events
	private string[] events; //{ "kills", "deaths","currentLvl"}; //max 255 hodnot
	private decimal[] eventsVal;
	private byte[] eventIt; //event increasing trend
	private int[] eventsExp;  // (ex rewards), - multipled by 1000 - optimized accurate float
	private int[] coinsExp; // (cx rewards), - multipled by 1000 - optimized accurate float
	private float[] eNbadgeV; // event next badge value - value of event needed for receiving next badge  - multipled by 1000 - optimized accurate float
	private bool impEvent = false; // very important event - synchronisation required
	//items
	private string[] slots = null;//{"item11", "item2", "item3"}; //vloz, odstran, nahrad - allows add a slot dinamically during the game
	private string[] itemsName = null;//{"item1","car2","car3","gun"};//nadefinovano
	private int[] itemsState = null;
	private string[][] itemSkillName = null;
	private float[][] itemSkillPerformance = null;
	private List<string> dependencies = null;

	private Dictionary<string, object> individualSaveData = new Dictionary<string, object>();
	private List<string> idKeysToSync = new List<string>(); // individualSaveData keys to sync
	private bool eventSyncRequired = false;

	//user
	private byte uKey, eKey, lcKey, outgoingLinks = 0;
	private ulong iu; // id user
	private uint ig; // id game
	private float[] ud = new float[2] { 0, 0 }; //userData = [coins, diamonds] - mirrored from server
	private string iHash = null;
	private float uProgress = 0;
	private float uStars = 0; // user stars
	private uint uBan = 0;
	private uint uExp = 0; //user experience (full sdk)
	private string networkAuthentication = null; // network user permission
	private string networkUniqueIdentificator = null;

	// cheatengine protection
	private short prCheckSumKey;
	private decimal prCheckSum; // checksum of real values


	private decimal localCurrency = 0M; //localCoin

	private FeatureBarIcons featureBar = new FeatureBarIcons();

	// web ranks
	private string[] webRanks = new string[]{"unlogged","casual","bronze","silver","gold","platinum","super"};
	private string[] rankColor = new string[]{"#87878787","#3399cc","#0ed801","#ffc107","#673ab7","#795548","#cddc39"};

	// similar to pureSDK
	private float minimumTimeScale; //hold volume / TimeScale value

	private bool mutedGame = false, activityService = false, autoSaveEnabled; //holds value of content limitation, is changeable only from this class
	private string currentScene, browserName, analyticsMode;
	private byte individualGameMode = 0;
	private byte asyncCallbacks = 2;

	//private byte giftBtnVisibility;

	private bool openingWebShop = false; // waiting for save callback before opening a shop (full sdk)

	private string _PWAStatus = "disabled";

	ServicesAdjustment servicesAdjustment = new ServicesAdjustment ();
	HGarterGui gui = new HGarterGui ();
	Protection protection = new Protection();
	HGarterBrowserServices browserServices = new HGarterBrowserServices ();
	HGarterPlayerPrefs playerPrefs = new HGarterPlayerPrefs();

	private enum _ConnReqType
	{
		SYNC=0,
		INIT=1, // init request - according to 
		SHOP=2, // shop req
		EDITOR=3, // init from editor
		EXPORT=4, // export conf data
		DELETE=5 // delete user account
	}

	//GUI
	private bool userDashboardAvailability, futureBoxAvailability, /*giftBtnAvailability,*/ enabledNotifications = true, rewardedAdsEnabled = false, progressBarVisibility = true;

	//Brand
	private string[] logoName = new string[3]{null,null,null}; // names of brand logos to load - 2 loga ve hre - logo[0], emblem[1] -"gamearter","pacogames_l","pacogames_e"
	private Texture2D[] logoTexture = new Texture2D[3];

	// -------------------------------------------- CODE -------------------------------------------- //
	/// <summary>
	/// Returns allowed minimum value of timeScale
	/// </summary>
	/// <returns>The minimum time scale.</returns>
	public float GetMinimumTimeScale(){
		return minimumTimeScale;
	}
		
	public string NetworkAuthentication(){
		return networkAuthentication;
	}

	public string NetworkUniqueUserId(){
		return networkUniqueIdentificator;
	}
		
	/// <summary>
	/// Returns whether rewarded ads are enabled at the place of the game
	/// </summary>
	/// <returns><c>true</c>, if ads are enabled, <c>false</c> otherwise.</returns>
	public bool RewardedAdsAvailability(){
		return rewardedAdsEnabled;
	}
		
	private void UpdateGarterGUIifAvailable(){
		// ----- Update dashboard ----- 
		if (asyncCallbacks == 0) {
			UpdateUserBoard ();
			UpdateFeatureBar ();
		}
	}
	/*
	private void HideGift(){ // soucasti skriptu v gift
		if (giftBtnAvailability && giftBtnVisibility == 0) {
			gui.HideGift ();
		}
	}*/

	/// <summary>
	/// Displaies the settings alert.
	/// </summary>
	public void SettingsAlert(bool display){
		featureBar.alerts[4] = (byte)((display) ? 1 : 0);
		UpdateGarterGUIifAvailable();
	}
	/// <summary>
	/// Displaies the shop alert.
	/// </summary>
	public void ShopAlert(bool display){
		featureBar.alerts[2] = (byte)((display) ? 1 : 0);
		UpdateGarterGUIifAvailable();
	}

	// ------ Lite SDK Features ----- //
	// Post On server... (via example scene)

	/// <summary>
	/// Change local currency funds about a certain value
	/// </summary>
	/// <returns>Current funds of the currency.</returns>
	/// <param name="change">Change value (+ = distributing, - for spending)</param>
	public decimal LocalCurrency(decimal change = 0M){
		if (change != 0M) {
			Log ("info","C| "+change);
			localCurrency += change;
			prCheckSum += change;
			UpdateUserBoard ();
		}
		return (localCurrency - lcKey);
	}
	private void SetLocalCurrency(decimal funds){ // after calling this, checksum must be recounted
		localCurrency = (funds + lcKey);
	}
		
	/// <summary>
	/// Sets the progress.
	/// </summary>
	/// <param name="progress">Progress.</param>
	public float UserProgress(float progress = 0){
		if (progress != 0 && progress != uProgress) {
			uProgress = progress;
			StartCoroutine (SafePostRequest (false,false,(error,response) => {
				if(!string.IsNullOrEmpty(error)) Debug.LogError("post user progress: "+error);
			}));
			UpdateGarterGUIifAvailable();
		}
		return uProgress;
	}

	/// <summary>
	/// SDK data management for getting initial SDK or shop sync SDK based data
	/// </summary>
	private void GetProgressDataReq<T>(_ConnReqType type, Action<T> callback){
		Log("info","Get Data request | type: "+type.ToString());
		string notifyMsg = "Loading data from storage...";
		string url = serverAddress;
		switch (type) {
		case _ConnReqType.INIT:
			url += (interpreter.Equals ('B')) ? "/basic" : "/get";
			break;
		case _ConnReqType.EDITOR: url += "/editor"; break;
		case _ConnReqType.SHOP:   url += "/shop"; break;
		default:
			Debug.LogError ("Unknown request");
			break;
		}
		NotificationModule (new object[]{false, notifyMsg});
		ServerConnJson<T> (url, SaveToString(new Identification (sdkVersion, GameId(), projectVersion, UserId(), iHash, interpreter)), callback);

		// u Guesta pouze prvni req, ktery je automaticky...
	}

	// Exports data on pressing export button
	public void ExportData<T>(string data, Action<T> callback){
		NotificationModule (new object[]{false, "Exporting data..."});
		string url = serverAddress;
		ServerConnJson<T> ((url += "/export"), SaveToString(new Identification (sdkVersion, GameId(), projectVersion, UserId(), data, interpreter)), callback);
		gui.IlustrateBrowserBox("clear", "Consider to clear saved user data to prevent errors (button available in Garter_initialized obj.");
	}

	// pro vsechny verze... (lite = obdoba z editoru)
	public void ClearData<T>(Action<T> callback){ // from editor button
		if (iu != uKey) { // guest / logged (user Id is not allocated so far)
			Debug.Log("Clear data");

			NotificationModule (new object[]{false, "Removing data..."});
			if (userAuthentizationState == 2) {
				string url = serverAddress;
				ServerConnJson<T> ((url+= "/del"), SaveToString(new Identification (sdkVersion, GameId(), projectVersion, UserId(), iHash, interpreter)), callback);
			} else if (userAuthentizationState == 1){
				playerPrefs.Clear (GameId().ToString(), projectVersion.ToString());
				callback ((T)Convert.ChangeType(new GarterWWW("ok",null,null), typeof(T))); //return callback
			} else {
				callback ((T)Convert.ChangeType(new GarterWWW(null,"Unknown user",null), typeof(T))); //return callback
			}
		} else {
			callback ((T)Convert.ChangeType(new GarterWWW(null,"Please, log in first to specify what user data to clear.",null), typeof(T))); //return callback
		}
	}
		
	private void PostProgressDataReq(string data, Action<string, string> callback){ // T as WWW
		Log("info","Post Data request");
		if (impEvent) impEvent = false;
		NotificationModule (new object[]{ false, "Saving data..." });
					
		// 2 typy dat - pouze herni / herni + sdk
		string hash = protection.Hash (GameId(), UserId(), data);
		if (userAuthentizationState == 2) { // --- SERVER --- //

			string url = serverAddress+"/sync";
			string dataWrapper = SaveToString (new _ServerObjPost (sdkVersion, GameId(), projectVersion, UserId(), hash, interpreter, data)); // data type post

			ServerConnJson<WWW> (url, dataWrapper, (wwwResp) => {
				// unwrap
				UnwrapProgressSyncReq(wwwResp.error, wwwResp.text);
				callback(wwwResp.error, "ok");
			});
		} else { // --- PLAYERPREFS --- //
			playerPrefs.SetGameData (GameId().ToString(), projectVersion.ToString(), data, hash, events); // unwrapping is not necessary (no response)

			if (openingWebShop) { // last data are saved, server has current values -> open shop (req with parameter 0 = sync)
				openingWebShop = false;
				OpenSdkWindowCallback ("shop"); // open shop window
			}
				
			idKeysToSync.Clear();
			callback(null, "ok");
		}
	}

	private void UnwrapProgressSyncReq(string error, string responseData){
		// coins, ban, progress
		if (!string.IsNullOrEmpty (error)) {
			Debug.LogWarning (error +" | "+ responseData);
			NotificationModule (new object[]{true, "Error - Server is down"});
			if (openingWebShop) {
				openingWebShop = false;
				gui.IlustrateBrowserBox ("error","Ops. Something went wrong. Please, let us feedback about this problem.");
			}
		} else {
			if (responseData != "wrong_hash") {
				lastSave = GetCurrentTimestamp ();
				idKeysToSync.Clear();

				if (openingWebShop) { // last data are saved, server has current values -> open shop (req with parameter 0 = sync)
					openingWebShop = false;
					OpenSdkWindowCallback ("shop"); // open shop window
				}

				ServerObjGet so = GetFromString<ServerObjGet> (responseData);
				string data = so.d;
				string hash = so.h;

				//Debug.Log("gameId: "+GameId()+" | userId: "+UserId()+" | hash: "+hash+" | data: "+data);
				//Debug.Log (protection.Hash (GameId(), UserId(), data, hash));
				if(protection.Hash (GameId(), UserId(), data, hash) == "ban") data = "ban";

				DefineAlertsVisibility (so.a);
				DefineNextBadgeValue((byte)0, so.nb);
				NotificationModule (null, so.b, so.lp, so.it, so.bu);

				/*
				if(interpreter.Equals('F')){
					if (type == 0) { // sync // - item has been unlocked
						Debug.Log ("uncomplete");// - nove DefineAlertsVisibility - nejen byla odemcena, AssemblyLoadEventArgs I byla zakoupena. zmena textu pred itemem...
					} else if(type == 2){ //type = 2 -> shop // - item has been bought - will be in different function
						Debug.Log ("uncomplete - item has been bought");
					}
				}*/
			
				if (data != "ban") {
					if (interpreter.Equals ('F')) { //full SDK
						SyncDataFull sdf = JsonConvert.DeserializeObject<SyncDataFull> (data);
						UpdateUserData (sdf.ud, sdf.b, sdf.p, sdf.s);
						uExp = sdf.uEx;
					} else { // Lite SDK
						SyncDataLite sdl = JsonConvert.DeserializeObject<SyncDataLite> (data);
						UpdateUserData (sdl.ud, sdl.b, sdl.p, sdl.s);
						if (sdl.c != 0) { // safety checksum
							// currency update
							decimal[] evr = LoadEventsVal();
							SetLocalCurrency(sdl.c);
							SaveEventsVal (evr); // must be after localCurrency - recount protection mech
							ForwardExternalCb(ExternalListener.CurrencyUpdate, sdl.c);
						}
					}
					UpdateGarterGUIifAvailable ();
				} else {
					BanUser (1, "Ban For server communication manipulation");
				}
			
			} else {
				Debug.LogError ("Ban For Wrong_hash");
				BanUser (1, "wrong_hash");
			}
		}
	}

	private void UnwrapProgressError(string error, string responseData){
		Debug.LogError (error + " | " + responseData);
		NotificationModule (new object[]{ true, "Error - Server is down" });
		if (!sdkInitialiized) {
			GameInitialized (error);
		}
	}

	private void UnwrapProgressGetReq(_ConnReqType type, string error, string responseData){ // Init / Editor / Shop(on close)
		// there is no direct call for get requests from dev side
		if (!string.IsNullOrEmpty (error)) {
			UnwrapProgressError (error, responseData);
		} else { Log ("info", "Unpack Wrapper (Get req) | "+ responseData); //no error

			if (responseData != "no_data" && responseData != "wrong_hash") { // data has been exported - unpack incoming data
				string data = null;
				string hash = null;

				// 1) ----- Unpack incoming data (replace this data for guests by previously saved data)----- 
				ServerObjGetInit so = GetFromString<ServerObjGetInit> (responseData);
				if (so.n != null && so.n.Length > 0) multiplNetwork = new string[2] { protection.PidD (so.n [0]), so.n [1] }; // network

					// GET ALL REQUIRED DATA IN JSON-STRING FORMAT
				if (type == _ConnReqType.EDITOR) { //EDITOR
					//Debug.Log (so.d);
					EditorProgress ep = FromJson<EditorProgress> (so.d); // webplayerData + serverData + editorData
					data = (interpreter.Equals ('F')) ? ToJson<ReceivedDataFull> (ep.rf) : ToJson<ReceivedDataLite> (ep.rl);
					WebglPlData wd = ep.g; //webglplayer data - set through SdkInitCallback
					SdkInitCallback (JsonUtility.ToJson (wd)); // webgl player data
					if (userAuthentizationState == 1) { // GUEST 
						data = GuestDataConverter ((byte)type, data, hash); // add missing objects (rewrite server data by playerprefs data)
					} // LOGGED USER - USE ONLY DATA FROM SERVER (DATA SAVE ALREADY, HASH IS NOT REQUIRED)

				} else { // PRODUCTION (Init / Shop sync)
					data = so.d;
					hash = so.h;
					if (userAuthentizationState == 2) { // logged
						if (protection.Hash (GameId (), UserId (), data, hash) == "ban")
							data = "ban";
					} else { // guest
						data = GuestDataConverter ((byte)type, data, hash); // add missing objects
					}
				}

				//Debug.Log ("so.i: "+so.i);
				DefineIconsVisibility (so.i);
				DefineAlertsVisibility (so.a);
				DefineNextBadgeValue ((byte)type, so.nb);

				//if(so.g == 0) HideGift (); // gift btn visibility

				NotificationModule (new object[]{ false, "Data was loaded" });
				lastSave = GetCurrentTimestamp ();

				string initStateError = null;

				if (data != "ban") {
					Log ("info","Update Data | "+type.ToString()+" | "+data);
					bool getReq = (type != _ConnReqType.SHOP);

					Debug.Log ("Unpack progress get req: "+data);

					if (interpreter.Equals ('F')) {
						ReceivedDataFull rd = JsonConvert.DeserializeObject<ReceivedDataFull> (data);
						if (rd.cx != null) coinsExp = MultiplyByThousand<int>(rd.cx); // move to get req
						if (rd.ex != null) eventsExp = MultiplyByThousand<int>(rd.ex); // move to get req
						if (rd.sl != null) slots = rd.sl;
						if (rd.i != null) itemsState = rd.i;
						if (rd.iv != null) itemSkillPerformance = rd.iv;
						if (rd.d != null) dependencies = rd.d;
						FillIndividualSaveData (rd.l);

						if (getReq) { // get / editor
							uExp = rd.uEx;
							initStateError = InitDataUpdate(rd.e);
							asyncCallbacks--;
							if (asyncCallbacks == 0) GameInitialized (initStateError);
						} else { // shop
							SdkWindowClosed("shop");
							UpdateUserData(rd.ud, rd.b, rd.p, rd.s);
							if (rd.e != null) {
								SaveEventsVal (rd.e);
								ForwardExternalCb (ExternalListener.EventExternalUpdate, "");
							}
							ForwardExternalCb (ExternalListener.CurrencyUpdate, rd.ud[0]);
						}

					} else { //Update Data Lite
						ReceivedDataLite rd = JsonConvert.DeserializeObject<ReceivedDataLite> (data);
						if (getReq) {
							if (rd.cx != null) coinsExp = MultiplyByThousand<int>(rd.cx);
							SetLocalCurrency (rd.lc);
							initStateError = InitDataUpdate(rd.e);  // must be below currency
							FillIndividualSaveData (rd.l);
							asyncCallbacks--;
							Log ("info", "set lite data | callbacks: " + asyncCallbacks);
							if (asyncCallbacks == 0)  GameInitialized (initStateError);
						} else { // shop
							UpdateUserData (rd.ud, rd.b, rd.p, rd.s);

							decimal[] evr = LoadEventsVal();
							SetLocalCurrency (rd.lc);
							if (rd.e != null) {
								SaveEventsVal (rd.e); // must be after localCurrency
								//garterLCB.EventUpdateViaShop();
								ForwardExternalCb (ExternalListener.EventExternalUpdate, "");
							} else {
								SaveEventsVal (evr); // must be after localCurrency
							}

							SdkWindowClosed("exchange");
							Log ("info","moving LC to game layer");
							ForwardExternalCb (ExternalListener.CurrencyUpdate, rd.lc);
							//garterLCB.CurrencyUpdate(rd.lc);
						}
					}
					UpdateGarterGUIifAvailable ();
				} else {
					BanUser (1, "Ban For server communication manipulation");
				}

			} else if (responseData == "no_data") { // no data has been exported
				string infoMsg = (!interpreter.Equals ('B')) ? "No data has been exported so far." : "No previous export requiring network found.";
				gui.IlustrateBrowserBox ("export", "\n"+infoMsg+" Please, make the export now. \n (via GameArter_Initialize object in hiearchy)");
				NotificationModule (new object[]{ true, "No data exported" });
				Debug.LogWarning ("GARTER SDK| no project configuration found. Please, make the export.");
			} else if (responseData == "wrong_hash") {
				Debug.LogError ("Ban For Wrong_hash");
				BanUser (1, "wrong_hash");
			}
		}
	}

	private void UnwrapProgressBasicReq(string error, string responseData){
		if (!string.IsNullOrEmpty (error)) {
			UnwrapProgressError (error, responseData);
		} else {
			if (responseData != "no_data" && responseData != "wrong_hash") { // data has been exported - unpack incoming data

				// basic sdk - // decode and save network

				BasicGet bg = GetFromString<BasicGet> (responseData);
				if (editorMode) {
					networkAuthentication = "editor";
					browserName = "editor";
				}
				multiplNetwork = new string[2]{ (protection.PidD (bg.n+bg.h)), bg.v.ToString () };
				asyncCallbacks--;
				if (asyncCallbacks == 0) { // img loaded already
					GameInitialized (null);
				}
					
			} else if (responseData == "no_data") { // no data has been exported
				string infoMsg = "No previous export requiring network found.";
				gui.IlustrateBrowserBox ("export", "\n"+infoMsg+" Please, make the export now. \n (via GameArter_Initialize object in hiearchy)");
				NotificationModule (new object[]{ true, "No data exported" });
				Debug.LogWarning ("GARTER SDK| no project configuration found. Please, make the export.");
			} else if (responseData == "wrong_hash") {
				Debug.LogError ("Ban For Wrong_hash");
				BanUser (1, "wrong_hash");
			}
		}
	}
		
	private byte _createdPostSafeReq = 0, _processedPostSafeReq = 0;
	private IEnumerator SafePostRequest(bool countableRequest, bool forcedRequest, Action<string, string> callback){
		_createdPostSafeReq++;
		yield return new WaitForSeconds (0.05f); // 100ms delay for direct call
		_processedPostSafeReq++;
		if (_createdPostSafeReq == _processedPostSafeReq) {
			Log("info","Garter | posting data");
			_createdPostSafeReq = 0;
			_processedPostSafeReq = 0;

			string requestEnabled = PostRequestFrequencyLimiter (countableRequest, forcedRequest);  // server is not down
			if (string.IsNullOrEmpty (requestEnabled)) {
				//Debug.Log ("Process request");
				decimal[] evr = null;
				bool isOk = true;
				if (eventSyncRequired || userAuthentizationState != 2) { // required for logged, or everytime for guest
					eventSyncRequired = false;
					evr = LoadEventsVal ();
					if (evr == null) {
						isOk = false;
						if (callback != null) callback ("Unverified data manipulation detected", null);
					}
				}

				if (isOk) {
					PostProgressDataReq (PostDataString (evr), (error, response) => {
						//Debug.Log(error+","+response);
						if (callback != null)
							callback (error, response);
					});
				}
			} else {
				if (callback != null)
					callback (requestEnabled, null);
			}
			
		} else {
			if(callback != null) callback (null, "aggregated");
		}
		yield break;
	}

	//lite server connection
	Int32 lastManualForcedRequestTime = 0;
	private string PostRequestFrequencyLimiter(bool countableRequest, bool forcedRequest){
		Int32 now = GetCurrentTimestamp ();
		if (countableRequest) { // request made by developer
			if (forcedRequest) {
				// limit for forced requests? 2 per minute
				if (now - lastManualForcedRequestTime > 60) {
					lastManualForcedRequestTime = now;
					return null;
				} else {
					return "Limit of forced post requests exceeded";
				}
			} else {
				if (now - lastTrackedManualSave > (saveFrequency [0] * 60)) {
					manualRequestsCounter = 0;
					lastTrackedManualSave = now;
				} else if(countableRequest){
					manualRequestsCounter++;
				}
				if (manualRequestsCounter < saveFrequency [1]) {   // saveFrequency [1] - number of enabled requests
					return null;
				} else {
					string warningMsg = "Limit for maximum number of post requests exceeded. There is possible to make only " + saveFrequency [1] + " requests per " + saveFrequency [0] + " minutes.";
					Debug.LogWarning (warningMsg);
					return warningMsg;
				}
			}
		
		} else { // request made by SDK
			if (forcedRequest) {
				return null;
			} else {
				Int32 timeDiff = now - lastSave; // time between this and last save
				// 1 request per 2 minutes
				if (timeDiff > saveFrequency [0] * 60) {
					return null;
				} else {
					return "Post ignored. No minimum time between SDK save freqwuency";
				}
			}
		}
	}
		

	// -------------------------------- data management start ----------------------------- //
	/// <summary>
	/// Gets user progress data.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="callback">Callback.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T GetData<T>(string key, Action<string,T> callback = null){
		if (!interpreter.Equals ('B')) {
			if (individualSaveData.ContainsKey (key)) {
				Type dataType = individualSaveData [key].GetType ();
				Type requestedDataType = typeof(T);
				if (dataType.Equals (requestedDataType)) {
					if (callback != null)
						callback (null, (T)individualSaveData [key]);
					return (T)individualSaveData [key];
				} else {
					//Debug.Log (dataType.Name+" vs "+requestedDataType.Name);
					T value;
					try {
						value = ((T)Convert.ChangeType (individualSaveData [key], typeof(T)));
						if (callback != null)
							callback (null, value);
						return value;
					} catch {
						try {
							string stringTest = individualSaveData [key].ToString ();
							value = JsonConvert.DeserializeObject<T> (stringTest);
							if (callback != null)
								callback (null, value);
							return value;
						} catch (Exception e1) {
							Debug.LogWarning ("Garter | Parsing failed, defautl value returned");
							if (callback != null)
								callback (e1.ToString (), default(T));
							return default(T);
						}
					}
				}
			} else {
				if (callback != null)
					callback (null, default(T));
				return default(T);
			}
		} else {
			Debug.LogError ("Storage feature is not available in Basic SDK mode");
			if (callback != null) callback ("Storage feature is not available in Basic SDK mode", default(T));
			return default(T);
		}
	}

	public void PostData<T>(string key, T value, Action<string, string> callback = null){ // called manually from liteSDK
		if (!interpreter.Equals ('B')) {
			string addOnListErr = SetIndividualGameData<T> (key, value);
			if (string.IsNullOrEmpty (addOnListErr)) {
				StartCoroutine (SafePostRequest (true, false, (error, response) => {
					if (callback != null)
						callback (error, response);
				}));
			} else {
				if (callback != null)
					callback (addOnListErr, null);
			}
		} else {
			Debug.LogError ("Storage feature is not available in Basic SDK mode");
			if (callback != null)
				callback ("Storage feature is not available in Basic SDK mode", null);
		}
	}

	public string SetIndividualGameData<T>(string key, T value){
		if (userAuthentizationState != 0) {
			// add / rewrite (RAM LAYER (keep up to date))
			if (individualSaveData.ContainsKey(key)) {
				individualSaveData [key] = value;
			} else {
				individualSaveData.Add (key, value);
			}

			if(!idKeysToSync.Contains(key)) idKeysToSync.Add (key);
			return null;
		} else {
			return "Unknown user. No user is logged in.";
		}
	}

	public void ClearDataKey<T>(string key, Action<string, T> callback = null){
		PostData<T> (key, default(T), (error, response) => {
			if (callback != null) callback (error, default(T));
		});
	}
	// -------------------- data management end ------------------------------------ //


	public bool IsFirstLoad(){
		return firstLoad;
	}
	public bool IsLoggedUser(){
		return (userAuthentizationState == 2);
	}
	public string GetServerAddress(){
		return serverAddress;
	}

	/// <summary>
	/// Gets the network id and ver. (Read only)
	/// </summary>
	/// <returns>[MultiplayerNetwork id, network version]</returns>
	public string[] GetMultiplayerNetwork(){
		// decipher over gprotect...
		return (multiplNetwork != null) ? multiplNetwork : null;
	}
		
	public byte UserRankIndex(){
		return (byte)Math.Ceiling ((double)uStars / 4);
	}
	public string UserRankName(){
		return webRanks [UserRankIndex()];
	}
	public string UserNick(){
		return user [0];
	}
	public string UserLang(){
		return user [1];
	}
	public string UserCountry(){
		return user [2];
	}
	/// <summary>
	/// Return user Photo (Texture2D)
	/// </summary>
	/// <returns>The user photo.</returns>
	public Texture2D UserImage(){
		return userPicture;
	}
		
	public uint GameId(){
		return (ig - uKey);
	}

	public ulong UserId(){
		return (iu - uKey);
	}
		
	// ---------------------------------- Micro features ------------------------------------- //
	private void SetUserImage(WWW www){
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.LogWarning("UserPhoto ERR | "+www.error);
		} else {
			userPicture = www.texture;
			UpdateUserBoard ();
		}
		asyncCallbacks--; // async operation of downloading game data and user image
		Log ("info","set user image | callbacks: "+asyncCallbacks);
		if (asyncCallbacks == 0) GameInitialized (null);
	}
		
	public void BlockGame(string info){
		Debug.LogWarning ("Game is being blocked for a reason of "+info);
		mutedGame = true;
		AudioListener.pause = true;
		AudioListener.volume = 0;
		Time.timeScale = 0;
	}

	private Int32 GetCurrentTimestamp(){
		return (Int32)(DateTime.UtcNow.Subtract(new DateTime(2018, 1, 1))).TotalSeconds;
	}
		
	// ------------------------------- Update GUI Start ------------------------------ //

	// ----- FEATURES BAR ICONS ----- //
	// fills data to iconsVisibility var - defines visible buttons in feature bar
	private void DefineIconsVisibility(byte[] visibility){ // visibility - from server - index 0 - 3 - leaderboard, badge, shop, share
		// leaderboard, badge, shop, login, settings, share
		if(visibility != null){
			bool fromServer = (visibility.Length == 4);
			switch (interpreter) {
			case 'B':
				featureBar.visibility = new byte[6]{ 0, 0, 0, 0, visibility [0], 1 };
				break;
			case 'L':
				if (fromServer) {
					featureBar.visibility = new byte[6]{ visibility[0], visibility[1], featureBar.visibility[2], (byte)((userAuthentizationState == 2) ? 0 : 1), featureBar.visibility[4], visibility[3] };
				} else {
					featureBar.visibility = new byte[6]{ 1, 1, visibility [0], 1, visibility [1], 1 }; // leaderboard, badge, shop, login, settings, share
				}
				break;
			case 'F':
				if (fromServer) {
					featureBar.visibility = new byte[6]{ visibility[0], visibility[1], visibility[2], (byte)((userAuthentizationState == 2) ? 0 : 1), featureBar.visibility[4], visibility[3] };
				} else {
					featureBar.visibility = new byte[6]{ 1, 1, 1, 1, visibility [0], 1 }; 
				}
				break;
			}
		}
	}
	// ----- FEATURES BAR ICONS ALERTS ----- //
	// define, on which icons aletr points should be visibled in featureBox
	private void DefineAlertsVisibility(byte[] conf){
		if (conf != null && conf.Length == 4) {
			featureBar.alerts = new byte[6] {
				conf [0], // leaderboard
				conf [1], // badges
				((featureBar.alerts [2] == 1) ? (byte)1 : conf [2]), // shop
				1, // login - is not visible, if logged in
				featureBar.alerts [4], // settings
				conf [3]  // share
			};
			UpdateGarterGUIifAvailable ();
		}
	}

	// ----- NOTIFICATIONS ----- //
	// notify error

	// ####### GUI requiring MonoBehaviour  #######

	private void NotificationModule(object[] connectionObj, string[] badges = null, uint[] leadeabord = null, string[] items = null, string[] bigbadgesImg = null, int spec = 0){
		//NotificationModule (null, new string[]{"Strelec","Valecnik","Badge 3"}, new uint[]{5,2,1}, new string[]{"Gun 6","Gun 8","Gun 21","Gun 16"}); //test showcase
		if (enabledNotifications) {
			Sprite[] sprites = Resources.LoadAll<Sprite>(@"sdkgui");
			Sprite theSprite = null;

			if (connectionObj != null) { 
				//server messages
				bool error = (bool)connectionObj[0];
				string message = (string)connectionObj [1];
				foreach (var sprite in sprites) {
					// error
					// else
					if (sprite.name == "sdkgui_6") {
						theSprite = sprite;
						break;
					}
				}
				gui.DisplayNotification(theSprite,message); //display immediatelly
				// destroy notification
				if (error) {
					gui.DisplayNotification(theSprite,message); //display immediatelly
					StartCoroutine (TimeOut (20, theSprite, "Destroy"));
				} else {
					StartCoroutine (TimeOut (2, theSprite, "Destroy"));
				}
			} else { 
				//notifications
				//spec: init = 0, sync = 1, shop = 2, editor = 3
				float delay = 0,
				visibleSec = 6;

				string text = "";
				if (badges != null) {
					text = "New badge: ";
					foreach (var sprite in sprites) {
						if (sprite.name == "sdkgui_23") {
							theSprite = sprite;
							break;
						}
					}
					int bAlerts = badges.Length;
					for (int a = 0; a < bAlerts; a++) {
						StartCoroutine (TimeOut(delay, theSprite, text+badges[a]));
						delay += visibleSec;
					}
					ForwardExternalCb (ExternalListener.ReceivedBadges, badges);
				}
				if (leadeabord != null) {
					foreach (var sprite in sprites) {
						if (sprite.name == "sdkgui_24") {
							theSprite = sprite;
							break;
						}
					}
					int lAlerts = badges.Length;
					for (int a = 0; a < lAlerts; a++) {
						StartCoroutine (TimeOut(delay, theSprite, "Between best "+leadeabord[a]+" players today / this week / this month"));
						delay += visibleSec;
					}
				}
				if (items != null) {
					// spec
					switch(spec){
					case 1:
						text = " bought";
						break;
					default:
						text = " unlocked";
						break;
					}
					foreach (var sprite in sprites) {
						if (sprite.name == "sdkgui_22") {
							theSprite = sprite;
							break;
						}
					}
					int iAlerts = items.Length;
					for (int a = 0; a < iAlerts; a++) {
						//Debug.Log ("NOTIFICATION | ITEM "+items[a]+text);
						StartCoroutine (TimeOut(delay, theSprite, "New item "+items[a]+text));
						delay += visibleSec;
					}
				}

				if (bigbadgesImg != null) {
					//Debug.Log ("Zobrazeni velkeho odznaku - dokoncit!");
				}
					
				// remove notification box
				if(badges!= null || leadeabord != null || items != null){
					StartCoroutine (TimeOut(delay, null, "Destroy"));
				}
			}
		}
	}
	private IEnumerator TimeOut(float time, Sprite sprite, string text){ //For changing notifications / destroying
		yield return new WaitForSeconds (time);
		gui.DisplayNotification (sprite, text);
		yield break;
	}


	private void UpdateUserBoard(){
		if (userDashboardAvailability) {
			gui.UserDashboard (user [0], userPicture, webRanks, rankColor, ud, uStars, uProgress, uExp, interpreter, (lcKey != 0), progressBarVisibility);
			//Log ("info", "Update Garter GUI (UpdateUserBoard)");
		}
	}
	/// <summary>
	/// Update feature bar.
	/// </summary>
	/// <param name="icons">Icons Visibility [LeaderBoard, Badges, shop, Settings, share].</param>
	/// <param name="notifications">Notifications for icons.</param>
	public void UpdateFeatureBar(){
		if (featureBar.visibility.Length > 1 && futureBoxAvailability) gui.FeatureBoxModule (featureBar.visibility, featureBar.alerts);
	}
		
	// ------------------------------- Update GUI End  ------------------------------- //


	// ---------------------------------------------------------------------------------- //

	//in production mode, all rewards 0 (protection from loading game without connection - playing with default data, the convert to account by registration), in editor, test data
	/*private void NoConnectionData(bool displayGUI = true){ //no connection // risk of data rewrite - stop game
		// Unused function
		if(displayGUI) gui.SdkDebugger ("warning","Server problem. no connection data.");
		
		if (lastSave == 0) {
			int gdl = events.Length;
			coinsExp = new int[gdl];
			if (interpreter.Equals ('F')) {
				eventsExp = new int[gdl];
				for (uint i = 0; i < gdl; i++) {
					eventsExp [i] = 0;
				}
			}
			for (uint i = 0; i < gdl; i++) {
				coinsExp [i] = 0;
			}
			networkAuthentication = "offline";
			multiplNetwork = new string[2]{protection.GetBackupPhotonId(), GameId().ToString()};

			OpenSdkWindow ("offline");
			SetLocalCurrency (0M);
		}
		DefineIconsVisibility(new byte[4]{ 0, 0, 0, 0});
		DefineAlertsVisibility(new byte[4]{ 0, 0, 0, 0});
	}*/
		
	public void PostDataManually(){
		StartCoroutine (SafePostRequest (true, true, null));
	}
		
	#if UNITY_EDITOR
	public void HEditorClearUserdata<T>(Action<T> callback){
		if(iHash != null){
			playerPrefs.Clear (GameId().ToString(), projectVersion.ToString());
			ClearData <WWW> ((wwwResp) => {
				callback ((T)Convert.ChangeType(new GarterWWW(wwwResp.text,wwwResp.error,null), typeof(T))); //return callback
			});
		} else {
			callback ((T)Convert.ChangeType(new GarterWWW(null,"Please, log in first to authetizate. (Securty reason)",null), typeof(T))); //return callback
		}
		
	}
	public void HEditorAllGuestData(){
		string[] eventsPP = playerPrefs.GetSaveEvents(GameId().ToString(), projectVersion.ToString());
		if (eventsPP != null) {
			Debug.Log ("DEFINED EVENTS (Diagnostic)");
			for (int i = 0; i < eventsPP.Length; i++) {
				Debug.Log (eventsPP[i]);
			}
		}
		Debug.Log ("SAVED DATA (Diagnostic)");
		string[] dataPP = playerPrefs.GetGameData (GameId ().ToString (), projectVersion.ToString());
		Debug.Log (dataPP[0]);
	}
	#endif
	/// <summary>
	/// Clear user data associated with the games.
	/// </summary>
	public void ClearDataUserReq(){ gui.ClearGameProgress (editorMode); } // req from public button  - restart saved data
	/// <summary>
	/// Private. Do not use.
	/// </summary>
	public void ClearDataUserConfirm(){ // confirmation of clear user data request - clear the data
		Log("info","ClearDataUserConfirm");

		ClearData <WWW> ((wwwResp) => {
			if (wwwResp.text == "ok") {
				gui.IlustrateBrowserBox ("clear", "Data was successfully cleared.");
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
				#endif

				if (!editorMode) {
					browserServices.GameRestart ();
				} else {
					// restart window for localhost / file://
				}

			} else {
				gui.IlustrateBrowserBox ("error", "Ohh, something went wrong.");
			}
		});
	}

	/// <summary>
	/// Downloads any data file
	/// </summary>
	/// <param name="url">URL.</param>
	/// <param name="callback">Callback.</param>
	public void DownloadDataFile(string url, Action<WWW> callback){
		if(!string.IsNullOrEmpty(url)) ServerConnFile (url, callback);
	}

	public enum AssetsManagementType
	{
		POST=7,
		GET=8,
		UPDATE=9,
		DUPLICATE=10,
		DELETE=11,
		RATE=12
	}
	public void AssetsManagement<T>(AssetsManagementType type, string data, Action<T> callback){
		Log("info","Asset request | type: "+type.ToString());
		string hash = protection.Hash (GameId(), UserId(), data);
		// request
		string url = serverAddress;
		switch (type) {
		case AssetsManagementType.POST:
			url += "/assets/post";
			break;
		case AssetsManagementType.GET:
			url += "/assets/get";
			break;
		case AssetsManagementType.UPDATE:
			url += "/assets/update";
			break;
		case AssetsManagementType.DELETE:
			url += "/assets/delete";
			break;
		case AssetsManagementType.RATE:
			url += "/assets/rate";
			break;
		}
		string dataWrapper = SaveToString (new _ServerObjPost (sdkVersion, GameId(), projectVersion, UserId(), hash, interpreter, data));
		ServerConnJson<T> (url, dataWrapper, callback);
	}

	//SERVER MODULE START
	private void ServerConnFile<T> (string url, Action<T> callback)  //1 - get, 2 - set, 0 - get img (sometimes, data = hash);
	{
		WWW www = new WWW (url);
		StartCoroutine (ServerCallback<T> (www, callback));
	}

	private Dictionary<string, string> headers = null;
	private void ServerConnJson<T> (string url, string data, Action<T> callback){
		WWWForm form = new WWWForm ();
		headers = form.headers;
		headers ["Content-Type"] = "application/json";
		headers ["Accept"] = "application/json";
		WWW www = new WWW (url, Encoding.UTF8.GetBytes (data), headers);
		StartCoroutine (ServerCallback<T> (www, callback));
	}

	private IEnumerator ServerCallback<T>(WWW www, Action<T> callback)
	{
		yield return www;
		if (typeof(T).FullName == "UnityEngine.WWW") {
			callback ((T)Convert.ChangeType(www, typeof(T)));
		} else { // - transport callback (LiteSDK)
			callback((T)Convert.ChangeType(new GarterWWW(www.text, www.error, www.texture), typeof(T))); //export, clear... - return response directly (without data management)
		}
	}
	// SERVER MODULE END
		
	private T[] MultiplyByThousand<T>(float[] array) where T : struct {
		T[] newArray = Array.ConvertAll<float, T>
		(
			array, 
			new Converter<float, T>
			(
				delegate (float f){
					return (T)Convert.ChangeType((Math.Round(f * 1000f)), typeof(T));
				}
			)
		 );
		return newArray;
	}
		
	private string GuestDataConverter(byte type, string serverData, string hash){
		string unifyData = null;
		// get data from playerprefs
		string[] dataPlayerPrefs = playerPrefs.GetGameData (GameId().ToString (), projectVersion.ToString()); // overeni hash u playerprefs dat?

		if (!string.IsNullOrEmpty (dataPlayerPrefs [0])) { // saved data exist
			// check data hash
			bool hashOk = (type == 1) ? (protection.Hash (GameId(), UserId(), serverData, hash) != "ban") : true; // server hash
			bool hash2Ok = (type == 1) ? (protection.Hash (GameId(), UserId(), dataPlayerPrefs [0], dataPlayerPrefs [1]) != "ban") : true; // playerprefs hash
			// 3) if ok, unify playerprefs data with server data
			if (hashOk && hash2Ok) {
				// 4) return unify data string

				if (interpreter.Equals ('L')) {
					string playerPrefsData = dataPlayerPrefs [0].ToString ();
					ReceivedDataLite serverDataL = JsonConvert.DeserializeObject<ReceivedDataLite> (serverData);
					ReceivedDataLite playerPrefsDataL = JsonConvert.DeserializeObject<ReceivedDataLite> (playerPrefsData);


					// move missing data from server to playerprefs
					playerPrefsDataL.cx = serverDataL.cx;

					unifyData = ToJson<ReceivedDataLite> (playerPrefsDataL);
				} else { // FullSDK
					ReceivedDataFull serverDataF = JsonConvert.DeserializeObject<ReceivedDataFull> (serverData);
					ReceivedDataFull playerPrefsDataF = JsonConvert.DeserializeObject<ReceivedDataFull> (dataPlayerPrefs [0]);

					// move missing data from server to playerprefs
					playerPrefsDataF.i = serverDataF.i;
					playerPrefsDataF.iv = serverDataF.iv;
					playerPrefsDataF.cx = serverDataF.cx;

					unifyData = ToJson<ReceivedDataFull> (playerPrefsDataF);
				}
			} else {
				unifyData = "ban";
			}
		} else { // no save data -> work with data received from server
			unifyData = serverData;
		}
		featureBar.alerts [3] = 1; // DISPLAY login icon
		return unifyData;
	}
		


	private void DefineNextBadgeValue(byte reqType, float[] nbValues){

		if (reqType == 1 || reqType == 3) { // init load - write badges

			if (nbValues != null && nbValues.Length > 0) {
				eNbadgeV = nbValues;//MultiplyByThousand<long> (nbValues); // event value of next badge (realtime badge module)
			} else { // no badge in the game
				int el = events.Length;
				for (int j = 0; j < el; j++) {
					eNbadgeV [j] = -1f;
				}
			}
		
		} else if (reqType == 0) { // sync - update next badges vals
		
			if (nbValues != null) {
				for (int nb = 0; nb < nbValues.Length; nb++) {
					// -1 - no other badges exists, -2 = use previous data
					if (nbValues [nb] != -2) eNbadgeV [nb] = nbValues [nb]; // event value of next badge (realtime badge module)
				}
			}
		
		} // else - shop - ignore
	}
		
	private string InitDataUpdate(decimal[] evValues){

		string errorResponse = null;
		if (evValues != null && evValues.Length == events.Length) { // no change
			SaveEventsVal (evValues);
		} else { // check - protection mechanism
			#if UNITY_EDITOR
			Debug.LogError ("Clash between current events and events of the loaded save. Solution - Check whether same events are set in game and on server. If so, clear saved data.");
			#endif
			if (userAuthentizationState == 1) { // GUEST
				decimal[] evFix = new decimal[events.Length];
				// get old events
				string[] oldEIdns = playerPrefs.GetSaveEvents(GameId().ToString (), projectVersion.ToString());
				if (oldEIdns != null) {
					for (int l = 0; l<events.Length; l++) {
						if (evValues != null && events [l] == oldEIdns [l]) { // same event name ID
							evFix [l] = evValues [l];
						} else {
							evFix[l] = (eventIt[l] != 1) ? 0 : 100000; // does not matter, no data badges for guests...
						}
					}
				} else {
					for (int l = 0; l<events.Length; l++) evFix[l] = (eventIt[l] != 1) ? 0 : 100000;
				}
				SaveEventsVal (evFix);
			} else { // logged - on server structure bug
				gui.IlustrateBrowserBox ("error","Ops. Something went wrong. Please, let us feedback about this problem.");
				errorResponse = "err-InitDataUpdate-logged";
			}
		}
		return errorResponse;
	}

	private void FillIndividualSaveData(Dictionary<string, object> data){
		if (data != null) {
			foreach (KeyValuePair<string,object> kvp in data) {
				if (individualSaveData.ContainsKey(kvp.Key)) {
					individualSaveData [kvp.Key] = kvp.Value;
				} else {
					individualSaveData.Add (kvp.Key, kvp.Value);
				}
			}
		}
	}
		
	private void UpdateUserData(float[] userData, byte ban, float progress, float stars){ //for node response only
		// logged / unlogged player...
		if (userAuthentizationState == 1) {
			Log ("info","Guest - veryfying playerprefs");
			// nebudu zatim davat do playerprefs - riziko...
			uExp = 0;
			uBan = 0;
			userData = new float[2]{0,0};
		} else {
			uBan = ban;
		}

		ud = userData;

		if (uBan != 0) { //check for ban - banned
			ud = new float[2]{0,0};
			Debug.LogError ("Ban from past (saved in db) | "+uBan);
			BanUser (3, (userAuthentizationState == 2) ? "logged" : "unlogged");
		}
		if (progress != 0) uProgress = progress;
		if (stars != 0) uStars = stars;
	}

	private void BanUser(byte reason, string note){
		if (!editorMode) {
			browserServices.GameBan (JsonUtility.ToJson (new BanInfo (reason, uBan, note)));
		} else {
			gui.IlustrateBrowserBox ("ban","BAN | You are banned for hacking | reason: "+reason+" | uBan: "+uBan+" | note:"+note);
			BlockGame ("Ban");
		}
	}
		
	private void SaveEventsVal(decimal[] ev){ // save to game sdk - check checksum

		int el = ev.Length;
		eventsVal = new decimal[el];
		System.Random rKey = new System.Random ();
		prCheckSumKey = (short)rKey.Next (1521, 27365); // static member
		prCheckSum = prCheckSumKey; // dynamic member
		for (int i = 0; i < el; i++) {
			if (ev [i] >= 0 && ev [i] < 4294967295) {
				eventsVal [i] = ev [i] + eKey;
				prCheckSum += ev [i];
			} else {
				Debug.LogError ("Event value out of rage");
			}
		}
		prCheckSum += (localCurrency - lcKey); // replace for GetLoacalCurrency()
		// -> prCheckSum for checks
	
	}

	private decimal[] LoadEventsVal(){ // verify values according to checkSum

		decimal eventsSum = prCheckSumKey;
		int el = eventsVal.Length;
		decimal[] evr = new decimal[el];
		for (int i = 0; i < el; i++) {
			decimal rValue = eventsVal [i] - eKey;
			evr [i] = rValue;
			eventsSum += rValue; // sum of pure amounts
		}
		eventsSum += (localCurrency - lcKey);
			
		decimal diff = Math.Abs(eventsSum - prCheckSum); // prCheckSum = checksum of game data, eventssum = counted checksum from events + currency
		if (diff > 1) {
			uBan = 2;
			if (editorMode) {
				gui.IlustrateBrowserBox ("ban", "Ban for hacking (data manipulation)");
			} else {
				Debug.LogError ("Ban For event values manipulation");
				BanUser (2, diff.ToString ());
			}
			return null;
		} else {
			return evr;
		}

	}
		
	// ------------------------------- Start Of Initialize ----------------------------- //

	public void Initialize(object[] initializeData){
		if (initializeData != null) { //Initialize complet

			if (System.DateTime.Now.Year< 0) { // actually never run but Unity doesn't know :)
				Debug.Log(new List<float>()); // because of Serializable and deserializable of float[] via Newtonsoft
				Debug.Log(new List<ulong>());
				Debug.Log(new List<bool>());
				Debug.Log (new List<Int32> ());
				Debug.Log (new List<Int32[]> ());
				Debug.Log (new List<Decimal> ());
				Debug.Log (new List<Decimal[]> ());
				Debug.Log (new Dictionary<string,object> ());
			}

			featureBar.alerts = new byte[]{ 0, 0, 0, 1, 0, 0 };

			interpreter = (char)initializeData [2];
			activeSdk = (bool)initializeData [3];
			minimumTimeScale = (float)initializeData [4];
			if (activeSdk) {
				Debug.Log ("GARTER SDK | INITIALIZATION | First Load: " + firstLoad + ", activeSDK: " + activeSdk+", unity: "+Application.unityVersion+", interpreter:"+interpreter+" ts: "+Time.timeScale+" sv:"+AudioListener.volume);

				System.Random rKey = new System.Random (); //generate a random key (make secret gameId & userId)
				uKey = (byte)rKey.Next (6, 52);
				eKey = (byte)rKey.Next (2, 31);
				ig = (uint)initializeData [0] + uKey;
				projectVersion = (byte)initializeData [1];
				analyticsMode = (string)initializeData[7];

				if (!interpreter.Equals ('B')) { //Full or Lite SDK

					DefineIconsVisibility ((byte[])initializeData [17]); // buttons for featureBox + progressBar
					events = (string[])initializeData [8];
					SaveEventsVal((decimal[])initializeData [9]);
					eventIt = ((byte[])initializeData [10]); // event increasing trend
					progressBarVisibility = (bool)initializeData [19]; // display progress bar
					autoSaveEnabled = ((bool)initializeData [21]);

					if (interpreter.Equals ('F')) { //Full SDK
						itemsName = (string[])initializeData [11];
						itemsState = (int[])initializeData [12];
						itemSkillName = (string[][])initializeData [13];
						itemSkillPerformance = (float[][])initializeData [14];
						dependencies = (List<string>)initializeData [15];
						slots = (string[])initializeData [16];

						//individualSaveData = (string)initializeData [20];
						individualSaveData.Add("default", initializeData [20]);


						lcKey = 0;
					} else { //Lite SDK
						lcKey = ((bool)initializeData [22]) ? (byte)rKey.Next (105, 231) : (byte)0;
						bool[] activeFeatures = (bool[])initializeData [18]; // display icons if feature bar
						if (!activeFeatures [0]) enabledNotifications = false;  //disabled notifications for liteSDK
					};
					lastTrackedManualSave = GetCurrentTimestamp ();
				} else { //Basic SDK
					progressBarVisibility = false;
					if ((bool)initializeData [6]) multiplNetwork = new string[2]{"",""}; //multiplayer required
					
					DefineIconsVisibility ((byte[])initializeData [8]);
					UpdateGarterGUIifAvailable ();
				}
					
				//protection
				if (!Application.isEditor) {
					string host = new System.Uri (Application.absoluteURL).Host;
					Log ("info","G host: " + host+" | Illegal running: "+protection.UrlProtection (host));
					if (protection.UrlProtection (host)) Debug.LogWarning ("PP is being disabled by developer");
					if ((bool)initializeData[5] && protection.UrlProtection (host)) { // block game
						BlockGame ("illegal host");
						SceneManager.LoadScene (0);
						OpenWebPage ("https://www.pacogames.com", null, false);
						gui.BlockMessage (host);
					} else { //run game - download user data -> gameplayer -> database
						if (host.IndexOf ("localhost") == -1 && host.IndexOf("file://") == -1) { // || file // Editro Mode = false
							servicesAdjustment.CacheGameSettings (); // cache default game settings
							browserServices.InitGameSignature (sdkVersion, interpreter, (uint)initializeData [0], (bool)initializeData [6], projectVersion);
						} else { //localhost = same behaviour as for editor
							editorMode = true;
							if (!interpreter.Equals ('B')) {
								OpenSdkWindow ("login");
							} else { // basic mode
								EditorMode ("guest");
							}
						}
					}
				} else { //is editor
					editorMode = true;
					if (!interpreter.Equals ('B')) {
						OpenSdkWindow ("login");
					} else {
						EditorMode ("guest");
					}
				}
				firstLoad = false;
			} else {
				Debug.LogWarning ("GARTER | SDK is not in active mode");
			}
		}
	}
	public void EditorMode(string init){ // skip/replace SdkInitCallback
		if (!interpreter.Equals ('B')) {
			if (init == "logged") {
				iu = (ulong)uKey + 2;
				userAuthentizationState = 2;
			} else if(init == "guest"){
				iu = (ulong)uKey + 1;
				userAuthentizationState = 1;
			}
			asyncCallbacks = (byte)((editorMode) ? 2 : 1);
			GetProgressDataReq<WWW>(_ConnReqType.EDITOR, (wwwResp) => UnwrapProgressGetReq(_ConnReqType.EDITOR, wwwResp.error, wwwResp.text));
		} else { // basic sdk
			if (multiplNetwork != null) { // get network... - basic sdk - get img and other data?
				asyncCallbacks = 2;
				GetProgressDataReq<WWW> (_ConnReqType.INIT, (wwwResp) => UnwrapProgressBasicReq(wwwResp.error, wwwResp.text));
			} else {
				asyncCallbacks = 1;
			}
			DownloadDataFile("https://www.pacogames.com/static/images/anonymous.png", result => SetUserImage(result));
		}
	}
		
	public void SdkInitCallback (string data){
		// ---- parse and save data start ----
		WebglPlData playerData = GetFromString<WebglPlData>(data);
		user = playerData.u;
		iu = (playerData.i + uKey);
		userAuthentizationState = (byte)((playerData.s != 0) ? 2 : 1); //number of stars is not 0 (registration === 1 star)

		UpdateUserData (playerData.ud, playerData.b, playerData.p, playerData.s);
		individualGameMode = playerData.gm;
		if(playerData.sf != null) saveFrequency = playerData.sf;
		
		outgoingLinks = playerData.ol; // outgoing links conf (are links clickable)
		referrer = playerData.rf; // clever top ifrarme url
		if (!string.IsNullOrEmpty(playerData.su)) serverAddress = playerData.su;

		if (playerData.sc != null && playerData.sc.Length != 0) rankColor = playerData.sc; //set new colors for ranks
		if (playerData.r != null && playerData.r.Length != 0) webRanks = playerData.r; //set new ranks
		if (playerData.l != null && playerData.l.Length != 0) logoName = playerData.l; //set branding logos
		if (!string.IsNullOrEmpty(playerData.bn)) browserName = playerData.bn; // set browser name
		if (playerData.a == 1) activityService = true; // enable game activity tracking
		if (!string.IsNullOrEmpty(playerData.na)) networkAuthentication = playerData.na; // set network authentication
		if(!string.IsNullOrEmpty(playerData.ni)) networkUniqueIdentificator = playerData.ni; // set network unique id
		if(!string.IsNullOrEmpty(playerData.pw)) {
			_PWAStatus = playerData.pw; // progressive web app status
		}

		rewardedAdsEnabled = playerData.ra;
		iHash = playerData.h; //protecting userId, attach to server call
		// ---- parse and save data end ----
		if (!editorMode) { // async callback 1
			asyncCallbacks = 2;

			if (!interpreter.Equals ('B')) { // Lite + full SDK
				GetProgressDataReq<WWW>(_ConnReqType.INIT, wwwResp => UnwrapProgressGetReq(_ConnReqType.INIT, wwwResp.error, wwwResp.text));
			} else if (multiplNetwork != null) { // basic mode - requires network
				GetProgressDataReq<WWW>(_ConnReqType.INIT, wwwResp => UnwrapProgressBasicReq(wwwResp.error, wwwResp.text));
			} else { // basic mode - does not require network
				asyncCallbacks--;
			}
		}

		//load user image - asyncCallback 2
		DownloadDataFile(playerData.pu, result => SetUserImage(result)); 
	}
		
	/// <summary>
	/// Encodes a string to be represented as a string literal. The format
	/// is essentially a JSON string.
	/// 
	/// The string returned includes outer quotes 
	/// Example Output: "Hello \"Rick\"!\r\nRock on"
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
	private string EncodeJsString(string s)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append("\"");
		foreach (char c in s)
		{
			switch (c)
			{
			case '\"':
				sb.Append("\\\"");
				break;
			case '\\':
				sb.Append("\\\\");
				break;
			case '\b':
				sb.Append("\\b");
				break;
			case '\f':
				sb.Append("\\f");
				break;
			case '\n':
				sb.Append("\\n");
				break;
			case '\r':
				sb.Append("\\r");
				break;
			case '\t':
				sb.Append("\\t");
				break;
			default:
				int i = (int)c;
				if (i < 32 || i > 127)
				{
					sb.AppendFormat("\\u{0:X04}", i);
				}
				else
				{
					sb.Append(c);
				}
				break;
			}
		}
		sb.Append("\"");

		return sb.ToString();
	}


	// ------------------------------- End Of Initialize ----------------------------- //
		
	//simple json
	private string SaveToString<T>(T value)
	{
		return JsonUtility.ToJson (value);
	}
	private T GetFromString<T>(string jsonString){
		if (!string.IsNullOrEmpty (jsonString)) {
			return JsonUtility.FromJson<T>(jsonString);
		} else {
			return default(T);
		}
	}

	//advanced json
	public string ToJson<T>(T value){
		return JsonConvert.SerializeObject(value);
	}
	public T FromJson<T>(string jsonstring){
		if (!string.IsNullOrEmpty (jsonstring)) {
			return JsonConvert.DeserializeObject<T> (jsonstring);
		} else {
			return default(T);
		}
	}
	private string PostDataString (decimal[] evr){
		Dictionary<string, object> dataToSync = null;
		if (userAuthentizationState == 2) {
			dataToSync = new Dictionary<string, object> ();
			for (int i = 0; i < idKeysToSync.Count; i++) {
				dataToSync.Add (idKeysToSync [i], individualSaveData [idKeysToSync [i]]);
			}
		} else { // Guest (PlayePrefs)
			//Debug.Log ("Individual data: "+individualSaveData);
			dataToSync = individualSaveData;
			// add playerprefs deserialization compatibility
			/*foreach (KeyValuePair<string,object> kvp in individualSaveData) {
				string stringValueFormat = ToJson (individualSaveData [kvp.Key]);
				dataToSync.Add (kvp.Key, (object)stringValueFormat);
			}*/
		}
			
		if (interpreter.Equals ('L')) {
			return JsonConvert.SerializeObject (new PostUserProgressLite (evr, LocalCurrency (), dataToSync, uProgress, projectVersion));
		} else { //Full SDK
			if (itemSync || userAuthentizationState != 2) { // save all
				itemSync = false;
				return JsonConvert.SerializeObject (new PostUserProgressFull (evr, slots, dataToSync, dependencies, uProgress, projectVersion));
			} else { // save changes only
				return JsonConvert.SerializeObject (new PostUserProgressFull (evr, null, dataToSync, null, uProgress, projectVersion));
			}
		}
	}
	// --------------------------------------- end of utilities for Server conunication -------------------------------------- //



	/******************************************* Work with dataStorage *******************************************/
	/// <summary>
	/// Gets the shop state of the item.
	/// </summary>
	/// <returns>Item is available for the player. (true = yes)</returns>
	/// <param name="itemName">Name of the item</param>
	public bool ItemAvailability(string itemName){ //read Only
		bool available = false;
		try{
			int index = System.Array.IndexOf (itemsName, itemName);
			if (index == -1) {
				Debug.LogWarning("Required item is not defined. Check, whether item "+itemName+" is created.");
			} else {
				if(itemsState [index] > -1){
					available = true;
				}
			}
		} catch {
			string msg = "";
			if (interpreter.Equals('F')) {
				msg = "No item attached in Items section";
			} else {
				msg = "Item section is available for GameArter mode only.";
			}
			Debug.LogWarning ("GameArter | "+msg);
		}
		return available;
	}

	/// <summary>
	/// Gets the item property value.
	/// </summary>
	/// <returns>The item property value. (read only)</returns>
	/// <param name="itemName">Item name.</param>
	/// <param name="property">Property.</param>
	public float ItemPropertyValue(string itemName, string property){ //vzdy zvysuji o jednu - ale muzu to i menit = prodej
		float value = 0;
		try{
			int index = System.Array.IndexOf (itemsName, itemName);
			if (index == -1) {
				Debug.LogWarning("Required item is not defined. Check, whether item "+itemName+" is created.");
				value = 0;
			}
			string[] itemPropertiesName = itemSkillName [index];
			//search for property value inside itemPropertiesName
			int propertyIndex = System.Array.IndexOf (itemPropertiesName, property);
			if (propertyIndex == -1) {
				Debug.LogWarning("Required property is not defined. Check, whether "+property+"  exists.");
			} else {
				value = itemSkillPerformance [index] [propertyIndex];
			}
		} catch {
			string msg = "";
			if (interpreter.Equals('F')) {
				msg = "No item *"+itemName+"* attached in Items section";
			} else {
				msg = "Item section is available for GameArter mode only.";
			}
			Debug.LogWarning ("GameArter | "+msg);
		}
		return value;
	}

	/// <summary>
	/// Stacks for selected items. E.g. weapons on action keys.
	/// </summary>
	/// <returns>Name of item</returns>
	/// <param name="index">Number of slot. (starts with 0)</param>
	/// <param name="addIt">Set <c>true</c> to add an item to the slot.</param>
	/// <param name="item">Name of item to add.</param>
	public string GetSlot(int index){ //spis by mnelo navracet nazev polozky, odesilam pozici
		string wantedItem = "-1";
			try {
				wantedItem = slots [index]; //if returns -1, the item is not existing
			} catch {
				Debug.LogWarning ("GameArter | slot with index "+index+" is not created. Max available index is "+(slots.Length - 1).ToString());
			}
			return wantedItem;
	}

	public int GetSlotOfItem(string itemStringId){
		return Array.IndexOf (slots, itemStringId);
	}

	public string SetSlot(int index, string item = null){ // FULL SDK ONLY
		string wantedItem = "";
		if (activeSdk) {
			if (!String.IsNullOrEmpty(item)) {
				if (System.Array.IndexOf (itemsName, item) != -1) { // item check (does the item exists?)
					if (index < slots.Length) {
						slots [index] = item;
						wantedItem = item;
						itemSync = true;
						if (autoSaveEnabled) StartCoroutine (SafePostRequest (false,true,null));
					} else {
						Debug.LogWarning ("GameArter | slot with index " + index + " is not created. Max available index is " + (slots.Length - 1).ToString ());
					}
				} else {
					Debug.LogWarning ("GameArter | Item " + item + " you are trying to add into slot does not exist.");
				}
			} else { //clear slot
				itemSync = true;
				slots [index] = null;
				if (autoSaveEnabled) {
					StartCoroutine (SafePostRequest (false,true,null));
				}
			}
		}
		return wantedItem;
	}

	/// <summary>
	/// Allows add another slot.
	/// </summary>
	/// <returns>Index of the created slot</returns>
	public int AddSlot(){ // FULL SDK ONLY
		string[] oldArrData = slots;
		int oldDataNum = slots.Length;
		if (activeSdk) {
			slots = new string[oldDataNum + 1];
			for (int i = 0; i < oldDataNum; i++) {
				slots [i] = oldArrData [i];
			}
			itemSync = true;
			if (autoSaveEnabled) StartCoroutine (SafePostRequest (false,true,null));
		}
		return oldDataNum; //=new slot Index
	}
	/// <summary>
	/// Get Full stackSlot
	/// </summary>
	/// <returns>Full Stack Slot</returns>
	public string[] GetSlots(){
		return slots;
	}

	/// <summary>
	/// Sets absolute value for an event. Analogy of Event window changing event value about X
	/// </summary>
	/// <returns><c>true</c>, if success, <c>false</c> otherwise.</returns>
	/// <param name="eventName">Event name.</param>
	/// <param name="absEventValue">Absolute value of event.</param>
	public bool SetEventAbs (string eventName, decimal absEventValue){
		if (activeSdk && absEventValue > 0) {
			int index = System.Array.IndexOf (events, eventName);
			if (index != -1) { // event Index
				Event(eventName, (absEventValue - eventsVal [index] - eKey));
				return true;
			} else {
				Event (eventName);
				return false;
			}
		} else {
			return false;
		}
	}
		
	/// <summary>
	/// Collection of game data (sum of kills or any other item, current level ...). Works as the ladder.
	/// </summary>
	/// <returns>Current value of required variable</returns>
	/// <param name="eventName">Defined Variable (kills, currentlvl ...)</param>
	/// <param name="addIt">Set <c>true</c> if you want to add an amount. Set<c>false</c> for reading only</param>
	/// <param name="amount">Select amount for adding.</param>
	public decimal Event (string eventName, decimal changeAbout = 0M){
		if (activeSdk) {
			decimal wantedVal;
			int index = System.Array.IndexOf (events, eventName);
			if (index != -1) { // event Index
				if (changeAbout != 0) { //writing
					Log ("info","EVENT| " + eventName + " | " + changeAbout);
					decimal changeAboutAbs = Math.Abs (changeAbout);

					if (eventIt [index] < 2) {

						if (eventIt [index] == 0) { //event with increasing trend - kills, deaths...
							eventsVal [index] += changeAboutAbs;
							prCheckSum += changeAboutAbs; // protection

							if (eNbadgeV [index] != -1 && (eventsVal [index] - eKey) >= (decimal)eNbadgeV [index]) impEvent = true; // received badge

						} else if (eventIt [index] == 1) { //event with decreasing trend  // time of lap
							if (eventsVal [index] >= (changeAboutAbs + eKey)) { // is > 0
								eventsVal [index] -= changeAboutAbs;
							} else {
								changeAboutAbs = (eventsVal [index] - eKey); // remove remaining value to 0
								eventsVal [index] = eKey; // set 0
								Debug.LogWarning ("GARTER | Event value "+eventName+" < 0 | Minimum event value is 0");
							}
							prCheckSum -= changeAboutAbs; // protection
							if (eNbadgeV [index] != -1 && (eventsVal [index] - eKey) <= (decimal)eNbadgeV [index]) impEvent = true; // received badge
						
						} else {
							Debug.LogWarning ("Unknown event trend " + eventIt [index]);
						}

						if (interpreter.Equals ('F')) uExp = (uint)Math.Round (uExp + (changeAboutAbs * (decimal)(eventsExp [index] / 1000))); //exp


						decimal currencyReward = changeAboutAbs * (decimal)(coinsExp [index] / (decimal)1000);
						if (lcKey == 0) { //rewards in GRT currency
							ud [0] = (float)((decimal)ud [0] + currencyReward );
						} else { // reward in local currency
							LocalCurrency(currencyReward); // coinsExp[index] = reward per 1000 units (= 1s, 1000kills...)
						}

						if (autoSaveEnabled || impEvent) StartCoroutine (SafePostRequest (false,impEvent,null));

						UpdateUserBoard ();
							
					} else if (eventIt [index] == 2) { // event with undefined trend
						if (changeAbout > 0) { // increase
							eventsVal [index] += changeAboutAbs;
							prCheckSum += changeAboutAbs; // protection
						} else { // decrease
							if (eventsVal [index] >= (changeAboutAbs+eKey)) {
								eventsVal [index] -= changeAboutAbs;
							} else {
								changeAboutAbs = (eventsVal [index] - eKey);
								eventsVal [index] = eKey;
								Debug.LogWarning ("Event value "+eventName+" < 0");
							}
							prCheckSum -= changeAboutAbs; // protection
						}
					} else {
						Debug.LogWarning ("Unknown event trend " + eventIt [index]);
					}

					eventSyncRequired = true;
				}
				// reading
				wantedVal = (decimal)(eventsVal [index] - eKey);
			} else {
				wantedVal = 0M;
				if (Application.isEditor) {
					Debug.LogError ("GameArter | Event *" + eventName + "* was not found. Check, whether *" + eventName + "* event is correct and whether the event is inserted");
				} else {
					Debug.LogWarning ("GameArter | Event *" + eventName + "* was not found. Check, whether *" + eventName + "* event is correct and whether the event is inserted");
				}
			}
			return wantedVal;
		} else {
			return 0M;
		}
	}

	/// <summary>
	/// Collection of dependencies for every referentItem. Writing and reading allowed.
	/// </summary>
	/// <returns>Whole dependency.</returns>
	/// <param name="referentItem">Referent item of the dependency.(key)</param>
	/// <param name="adjustDependency">Set to <c>true</c> if you want to adjust dependency of referentItem</param>
	/// <param name="dependency">Set new dependency or let null for remove current dependency of referentItem</param>
	public void SetAccessory(string referentItemId, string accessoryValue = null){ // FULL SDK ONLY
		Log("info","Set accessory | "+referentItemId+" | "+accessoryValue);

		object[] accessoryInfo = (object[])FindInDependencies (referentItemId);

		int dependencyIndex = (int)accessoryInfo [0];
		if (dependencyIndex != -1 && activeSdk) {
			dependencies.RemoveAt (dependencyIndex);
			if (!string.IsNullOrEmpty (accessoryValue)) dependencies.Insert(0,referentItemId+"#"+accessoryValue);
			itemSync = true;
			if (autoSaveEnabled) StartCoroutine (SafePostRequest (false,true,null));
		}

	}

	public string GetAccessory(string referentItemId){
		object[] accessoryInfo = (object[])FindInDependencies (referentItemId);
		return (string.IsNullOrEmpty((string)accessoryInfo[1])) ? null : (string)accessoryInfo[1];
	}

	private object FindInDependencies(string referentItemId){
		int dependenciesLength = dependencies.Count; // number of items with associated dependencies
		int dependencyIndex = -1;
		string accessoryValue = null;

		for (int i = 0; i < dependenciesLength; i++) { //search for key
			string fullDSlot = dependencies [i];
			try{ //throws error, if referentItem is without dependency
				int splitterPos = fullDSlot.IndexOf ("#");
				if(referentItemId == fullDSlot.Substring (0, splitterPos)) {
					dependencyIndex = i;
					accessoryValue = fullDSlot.Substring (splitterPos).Substring (1);
					break;
				} 

			} catch { //empty slot in a dependency... ?(je to list...)
				dependencies.RemoveAt (i); // dependency does not contain dependency key -> remove (it should not occure teoretically)
				dependenciesLength = dependencies.Count;
				i--;
			}
		}

		return new object[2]{dependencyIndex, accessoryValue};
	}
		
	// ------------------------------------------------------------------------------------------------------- //
	#if UNITY_EDITOR
	private static bool garterSdkCoreLoaded = false;
	#endif
	public static Garter I
	{
		get
		{
			_g = FindObjectOfType<Garter>();
			if (_g == null) { //Garter resource is not inserrted in the scene
				_g = Instantiate (Resources.Load<Garter> ("Garter_sdk"));  //loads Garter resource to every scene
				if (_g == null) {
					Debug.LogErrorFormat ("garter_sdk prefab is not in a Resources folder.");
				}
				try {
					DontDestroyOnLoad (_g.gameObject);
				} catch {
					Debug.LogError ("GARTER | Catched problem during sdk initialization.");
				}
				_g.name = "GameArter_sdk"; //private const string GARTER = "GameArter_sdk(garter.cs)";
				#if UNITY_EDITOR
				garterSdkCoreLoaded = true;
				if(GameObject.Find("Garter_sdk") != null){
					Debug.LogError("Remove Garter_sdk object from hiearchy");
				}
				#endif
			} else {
				#if UNITY_EDITOR
				if(!garterSdkCoreLoaded){
					Debug.LogError ("Remove Garter_sdk object from hiearchy. The script will be loaded automatically.");
				}
				#endif
			}
			return _g;
		}
	}
	private static Garter _g;


	// ------------------------------------------ Basic SDK ------------------------------------------- //
	/// <summary>
	/// Sets the fullcreen.
	/// </summary>
	public void Fullcreen(){
		if (!Application.isEditor) {
			browserServices.Fullscreen ();
		} else {
			Debug.LogWarning ("Fullscreen function is not available in editor mode");
		}
	}

	/// <summary>
	/// Loads and runs a certain animation in gameplayer
	/// </summary>
	/// <param name="animation">Animation.</param>
	public void RunStoryAnimation(int animation){
		if (!editorMode) {
			browserServices.StoryAnimation (animation);
		} else {
			Debug.LogWarning ("RunStoryAnimation is not available in editor mode");
		}
	}

	public string GetBrowserName(){
		return browserName;
	}
		
	public bool IsGameMuted(){
		return mutedGame;
	}
		
	// ######## Basic SDK #######
	// ######## REDIRECT START #######
	public object[] BrandModule(byte brandIndex, Texture2D texture = null){ // for dynamic branding only (does not apply for individual type)
		object[] response = null;
		if (texture == null) { // get
			response = new object[] {
				logoName [brandIndex],
				logoTexture [brandIndex]
			};
		} else { // set (save downloaded logo to textures)
			logoTexture[brandIndex] = texture;
			response = new object[] {
				logoName [brandIndex],
				logoTexture [brandIndex]
			};
		}
		return response;
	}

	public bool ClickableLink(string brand){ // soucast brandModule...
		bool clickable = false;
		switch (outgoingLinks) {
		case 1: // All disabled
			clickable =  false;
			break;
		case 2: // pg + ga disabled
			clickable =  ((brand.ToLower ().IndexOf ("brand") > -1) || (brand.ToLower ().IndexOf ("gamearter") > -1)) ? false : true;
			break;
		case 3: // pacogames disabled
			clickable =  (brand.ToLower ().IndexOf ("brand") > -1) ? false : true;
			break;
		case 4: //gamearter disabled
			clickable =  (brand.ToLower ().IndexOf ("gamearter") > -1) ? false : true;
			break;
		case 5:  // individual disabled
			clickable =  (brand.ToLower ().IndexOf ("individual") > -1) ? false : true;
			break;
		default: // all enabled
			clickable =  true;
			break;
		}
		return clickable;
	}
	/// <summary>
	/// Should be a certain content blocked? (Read only)
	/// </summary>
	/// <returns><c>true</c>, if content was limited, <c>false</c> otherwise.</returns>
	public byte IndividualGameMode(){
		return individualGameMode;
	}
	/// <summary>
	/// Opens webpage in a new or sam tab.
	/// </summary>
	/// <param name="openInNewTab">If set to <c>true</c> open in new tab.</param>
	/// <param name="url">URL.</param>
	public void OpenWebPage(string url, string brand = "individual", bool toNewTab = true){ // brand = logo type - web / gamearter / individual
		bool enabledLink = true;
		switch (outgoingLinks) {
		case 1:
			enabledLink = false;
			break;
		case 2:
			enabledLink = (brand.IndexOf ("brand") != -1 || brand.IndexOf ("pacogames") != -1) ? false : true;
			break;
		case 3:
			enabledLink = (brand.IndexOf ("brand") != -1) ? false : true;
			break;
		case 4:
			enabledLink = (brand.IndexOf ("gamearter") != -1) ? false : true;
			break;
		case 5:
			enabledLink = (brand.IndexOf ("individual") != -1) ? false : true;
			break;
		}

		string redirectUrl = "https://api.gamearter.com/game/redirect-module?g=" + GameId () + "&r=" + referrer + "&l=" + brand + "&u=" + url+"&p=dwebgl&v="+sdkVersion; // platform = desktopwebgl
		Log ("info","OpenWebPage | " + brand + ", url: "+url+" | "+redirectUrl);
		if (enabledLink) {
			#if UNITY_EDITOR
			toNewTab = false;
			#endif
			browserServices.Redirect (redirectUrl, toNewTab);
				
		} else {
			Debug.Log ("Redirect link to " + redirectUrl + " has been blocked");
		}
	}

	// ######## REDIRECT END #######
	// ######## ANALYTICS START #######
	private void OnEnable()
	{
		SceneManager.sceneLoaded += sceneChanged;
	}
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= sceneChanged;
	}
	private void sceneChanged(Scene scene, LoadSceneMode mode)
	{
		if (activeSdk) {
			if(analyticsMode != "off") {
				currentScene = scene.name.ToString();
				if (!editorMode) {
					browserServices.AnalyticsRequest (analyticsMode, currentScene, null, null, null, 0); 
				} else {
					Debug.Log ("GARTER SDK | Browser Services | Analytics (Info msg) | Current scene: "+currentScene+" | Feature works in GameArter gameplayer only.");
				}
			}
				
			// change of a scene - check availability of garter GUI
			bool[] garterGuiAvailability = gui.GarterGuiAvailability();
			userDashboardAvailability = garterGuiAvailability [0];
			futureBoxAvailability = garterGuiAvailability[1];
			//giftBtnAvailability = garterGuiAvailability [2];

			// update garter gui, if available
			UpdateGarterGUIifAvailable ();
		}
	}
		
	/// <summary>
	/// Allows send individual items (cars, guns) to Analytics. Scene is attached automatically.
	/// </summary>
	/// <param name="itemName">Item name.</param>
	public void AnalyticsEvent(string action, string category = null, string label = null, int value = 0){
		if (!editorMode) {
			browserServices.AnalyticsRequest (analyticsMode, currentScene, category, action, label, value);
		} else {
			Debug.Log ("GARTER | Analytics (info msg) | Posting "+ action + " / "+ label + " / "+ value+" to analytics (info msg) | NOTE: Feature works in GameArter gameplayer only.");	
		}
	}
	// ######## ANALYTICS END #######


	// ######## AD START #######

	/// <summary> Calls an ad. </summary>
	/// <param name="num">
	/// 2 = single ad on call;
	/// 1 = ad in a loop (for singleplayer only);
	/// 0 = unique ad mode - will be selected by admins
	/// </param>
	public void CallAd(int num = 0) { //calling ads - nums: 0 - select ad automatically / 1 - ads in a lopp (for singleplayer mode), 2 - ads on a request
		if (!editorMode) {
			browserServices.AdRequest (num, mutedGame, Cursor.lockState.ToString (), Cursor.visible);
		} else {
			OpenSdkWindow ("ad");
		}
	}
	public void RewardedAd(){
		//OpenSdkWindow ("ad"); // mute game
		if (!editorMode) {
			browserServices.AdRequest (6, mutedGame, Cursor.lockState.ToString (), Cursor.visible);
		} else {
			OpenSdkWindow ("ad"); // mute game
			HRewardedAdCb ("success");
		}
	}
	/// <summary>
	/// Do not use. Conn sync internal function with public state
	/// </summary>
	/// <param name="state">State.</param>
	public void HRewardedAdCb(string state){
		if (!editorMode) {
			SdkWindowClosed ("ad");
		}
		ForwardExternalCb(ExternalListener.RewarededAdState, state);
	}

	public void InstallAsPWA<T>(Action<T> callback){ // listener install
		// install internal listener
		AddCatchCbListener< T >(CachedListener._PWAState, callback);
		// send req
		if (!editorMode) {
			browserServices.InstallAsPWA ();
		} else {
			// State disabled
			PwaCb("disabled");
			OpenSdkWindow ("PWA");
		}
	}

	public void PwaCb(string state){
		if (state != "enabled" && state != "disabled") {
			ForwardCachedCb(CachedListener._PWAState, state); // installed...
		} else {
			_PWAStatus = state;
			ForwardExternalCb (ExternalListener.PWAState, state);
		}
	}

	public string GetStatePWA(){
		return _PWAStatus;
	}
		
	public void CreateLoadingScreen(string assetName, Texture assetImage = null, bool activeProgress = false){
		gui.CreateLoadingScreen (assetName, assetImage, activeProgress);
	}
	/// <summary>
	/// Current state in percentages of the asset loading
	/// </summary>
	/// <param name="progress">Progress.</param>
	public void UpdateLoadingScreen(float progress){
		gui.UpdateLoadingScreen (progress);
	}
	/// <summary>
	/// Removes the loading screen.
	/// </summary>
	public void RemoveLoadingScreen(){
		gui.RemoveLoadingScreen ();
	}

	private void GameInitialized(string state){
		if(!interpreter.Equals('B')) SdkWindowClosed ("login");
		sdkInitialiized = true;
		UpdateGarterGUIifAvailable ();
		ForwardExternalCb (ExternalListener.SdkInitialized, state);

		if (!editorMode) {
			if (state == null) {
				browserServices.GameInitialized ("ok");
			} else {
				browserServices.GameInitialized (state);
			}
			if(GetStatePWA()!="disabled") PwaCb("enabled");
		} else {
			gui.DestrolLoginBox ();
			PwaCb("enabled");
		}
	}
		

	/// <summary>
	/// Sdks the window opened.
	/// </summary>
	/// <param name="window">Window.</param>
	public void OpenSdkWindow(string window){
		#if UNITY_EDITOR
		if (GameObject.Find ("GameArter_sdk") == null) {
			Debug.LogError ( "Garter SDK must be initialized before calling its functions");
		}
		#endif

		if (window == "settings" || window == "shop" || window == "screenshot" || window == "gift" || window == "exchange") { // webwindows
			switch(window){
			case "settings":
				ForwardExternalCb (ExternalListener.ExternalSettingsButtonPressed, "");
				ClearAlertNotification (window);
				break;
			case "shop":
				if (interpreter.Equals ('F')) {
					openingWebShop = true;
					Log ("info","OPEN SHOP REQ");
					StartCoroutine (SafePostRequest (false,true,null));
				} else { // Lite SDK
					ForwardExternalCb (ExternalListener.ExternalShopButtonPressed, "");
				}
				ClearAlertNotification (window);
				break;
			case "exchange":
				openingWebShop = true;
				Log ("info","DataSynchronisationRequiredPost | OpenSdkWindow | exchange");
				StartCoroutine (SafePostRequest (false,true,null));
				break;
			/*
			case "gift": // in construction
				if (editorMode) {
					gui.IlustrateBrowserBox (window, "Gift received");
				} else {
					Application.ExternalEval ("Garter.game.openWindow("+JsonUtility.ToJson (new OpenWindowConf ("gift", 0))+")");
				}
				// remove gui
				break;
			case "screenshot":
				if (multiplNetwork == null) {
					Time.timeScale = minimumTimeScale;
				}
				break;*/
			}
		} else { // game window
			if (!editorMode) {
				if (window != "ad" && window != "PWA") {
					string data = null;
					if(window != "profile") {
						data = JsonUtility.ToJson (new OpenWindowConf (window, ClearAlertNotification (window)));
					} else {
						data = JsonUtility.ToJson(new UserBilance("profile", ud[0], ud[1], uStars, uProgress));
					}
					browserServices.OpenModule (data);
				}

			} else { // editor mode
				SdkWindowOpened(window); // mute game
				switch (window) {
				case "ad":
					gui.IlustrateBrowserBox ("ad", "Ad is being displayed");
					break;
				case "PWA":
					gui.IlustrateBrowserBox ("PWA", "Install game as App service is displayed");
					break;
				case "login":
					gui.EditorLogin ();
					ClearAlertNotification (window);
					break;
				case "badge":
					Application.OpenURL ("https://www.gamearter.com/modules/badges/"+GameId()+"/"+projectVersion);
					gui.IlustrateBrowserBox (window, "Badges box");
					ClearAlertNotification (window);
					break;
				case "leaderboard":
					Application.OpenURL ("https://www.gamearter.com/modules/leaderboard/"+GameId()+"/"+projectVersion);
					gui.IlustrateBrowserBox (window, "Leaderboard box");
					ClearAlertNotification (window);
					break;
				case "profile":
					Application.OpenURL ("https://www.gamearter.com/modules/profile/"+GameId());
					gui.IlustrateBrowserBox (window, "user profile");
					break;
				case "share":
					//Application.OpenURL ("https://www.gamearter.com/game_config/badges.php?g="+GameId()+"&u="+UserId());
					gui.IlustrateBrowserBox (window, "share box");
					ClearAlertNotification (window);
					break;
				case "offline": // enables converstion between local and paco currency
					//Application.OpenURL ("https://www.gamearter.com/game_config/badges.php?g="+GameId()+"&u="+UserId());
					gui.IlustrateBrowserBox (window, "Connection error. We were not able to connect in with server.");
					break;
				case "discussion":
					Application.OpenURL ("https://www.gamearter.com/modules/comments/"+GameId());
					gui.IlustrateBrowserBox (window, "discussion module");
					break;
				case "report":
					gui.IlustrateBrowserBox (window, "report module");
					break;
				case "development":
					gui.IlustrateBrowserBox (window, "development module");
					break;
				case "video":
					Application.OpenURL ("https://www.gamearter.com/modules/video/"+GameId());
					gui.IlustrateBrowserBox (window, "video module");
					break;
				case "gameinfo":
					Application.OpenURL ("https://www.gamearter.com/modules/controls/"+GameId());
					gui.IlustrateBrowserBox (window, "gameinfo module");
					break;
				case "moregames":
					gui.IlustrateBrowserBox (window, "moregames module");
					break;
				default:
					gui.IlustrateBrowserBox (window, "unknown req. Please, write windown name parameter properly.");
					break;
				}
			}
		}
		UpdateGarterGUIifAvailable ();
	}
		
	private void OpenSdkWindowCallback(string window){
		if (window == "shop") {
			if (editorMode) {
				gui.IlustrateBrowserBox (window, window+" box. Purchases will take effect on closing this window.");
				// mute game
				if (interpreter.Equals ('F')) {
					SdkWindowOpened ("shop");
				} else { // lite sdk
					SdkWindowOpened("exchange");
				}

				string shopURL = "https://www.gamearter.com/modules/shop/" + GameId () + "/" + projectVersion+"#pl=unityEditor&uid="+UserId()+"&pass="+iHash;
				Debug.Log ("SHOP ADDRESS: "+shopURL);
				Application.OpenURL (shopURL);

			} else {
				browserServices.OpenModule(JsonUtility.ToJson (new OpenWindowConf ("shop", ClearAlertNotification ("shop"))));
			}
		}
	}
	// called from browser - muting game via this
	public void SdkWindowOpened(string windowName){
		mutedGame = true;
		servicesAdjustment.ModuleWindowOpened(windowName);
		Log("info","SdkWindowOpened ("+windowName+") | MUTED? "+mutedGame+" | audio: "+AudioListener.volume+" |ts: "+Time.timeScale);
	}

	public void SdkWindowChanged(string window){
		servicesAdjustment.ModuleWindowChanged(window);
		Log("info","SdkWindowChanged ("+window+") | MUTED? "+mutedGame+" | audio: "+AudioListener.volume+" |ts: "+Time.timeScale);
	}

	public void SdkWindowClosed(string window = null){ // unmute game
		servicesAdjustment.ModuleWindowClosed (window); // ad
		mutedGame = false;
		Log("info","SdkWindowClosed ("+window+") | MUTED? "+mutedGame+" | audio: "+AudioListener.volume+" |ts: "+Time.timeScale);

		// retrun state to cb
		if(editorMode&&window == "PWA"){
			PwaCb("dismissed");
			PwaCb("enabled");
		}
	}
		
	private byte ClearAlertNotification(string window){
		int featureIconIndex = 0;
		switch (window){ // hide alerts
		case "leaderboard":
			featureIconIndex = 0;
			break;
		case "badge":
			featureIconIndex = 1;
			break;
		case "shop":
			featureIconIndex = 2;
			break;
		case "login":
			featureIconIndex = 3;
			break;
		case "settings":
			featureIconIndex = 4;
			break;
		case "share":
			featureIconIndex = 5;
			break;
		}
		byte alertState = featureBar.alerts [featureIconIndex];
		featureBar.alerts[featureIconIndex] = 0; // clear notification
		UpdateGarterGUIifAvailable();
		return alertState;
	}

	public void GetUserBilance(string str){ // from browser only
		browserServices.UserBilanceCallback (JsonUtility.ToJson (new UserBilance ("profile", ud [0], ud [1], uStars, uProgress)));
	}

	/*
	public void SdkModulSync(string window){
		byte state = ClearAlertNotification (window);
		if (state > 0) {
			Application.ExternalEval ("Garter.game.moduleSyncRes("+JsonUtility.ToJson(new OpenWindowConf(window, state))+")");
		}
	}
	*/

	public void SyncShop(string hash){ // hash is default
		//Debug.Log("-- Sync shop req --");
		if (editorMode) hash = iHash;
		if (userAuthentizationState == 2) {
			GetProgressDataReq<WWW> (_ConnReqType.SHOP, wwwResp => UnwrapProgressGetReq (_ConnReqType.SHOP, wwwResp.error, wwwResp.text));
		} else {
			SdkWindowClosed ("shop");
		}
	}
		
	[System.Serializable]
	internal class UserBilance
	{
		public string w; // window
		public float c; // coins
		public float d; // diamonds
		public float s; // stars
		public float p; // progress
		public UserBilance(string window, float coins, float diamonds, float stars, float progress){
			this.w = window;
			this.c = coins;
			this.d = diamonds;
			this.s = stars;
			this.p = progress;
		}
	}
		
	[System.Serializable]
	internal class GameSetting // set game before / after displayed ad
	{
		public bool m; //muteGame
		public float sv; //soundVolume
		public byte sp; //soundPause
		public float t; //timescale
		public string f; //focusManager
		public byte c; //cursor visibility
		public GameSetting(bool mutedGame, float soundVolume, byte soundPause, float gameTimescale, string focusManager, byte cursorVisibility){
			this.m = mutedGame;
			this.sv = soundVolume;
			this.sp = soundPause;
			this.t = gameTimescale;
			this.f = focusManager;
			this.c = cursorVisibility;
		}
	}

	[System.Serializable]
	internal class BanInfo //attaching to CallAd
	{
		public byte r; // reason
		public uint t; // ban time
		public string n; // note
		public BanInfo(byte reason, uint banTime, string note){
			this.r = reason;
			this.t = banTime;
			this.n = note;
		}
	}
		
	// ######## AD END #######

	// ######## GAME MANAGEMENT START #######

	private void OnApplicationFocus( bool hasFocus )
	{
		if (activityService) {
			//SdkWindowClosed ("ad");
			if (!editorMode) {
				browserServices.ActivityPing(1);
			} else {
				//Debug.Log ("Browser Services | Game is active");
			}
		}
	}
	private void OnApplicationPause( bool pauseStatus )
	{
		if(debugMode) Debug.Log ("OnApplicationPause: "+pauseStatus);
		if (activityService) {
			//OpenSdkWindow ("ad");
			if (!editorMode) {
				browserServices.ActivityPing(0);
			} else {
				//Debug.Log ("Browser Services | Game is not active");
			}
		}
			
		if (interpreter.Equals ('F') && autoSaveEnabled) {
		
			if (userAuthentizationState == 2) {
			
			} else {
				PlayerPrefs.Save ();
			}

		} else if (interpreter.Equals ('L')) {
			if(sdkInitialiized) ForwardExternalCb (ExternalListener.PossibleGameExit, "");
		} 
	}
	private void OnApplicationQuite( )
	{
		if(debugMode) Debug.Log ("OnApplicationQuite");
		if(sdkInitialiized) ForwardExternalCb (ExternalListener.PossibleGameExit, "");
	}

	// for a case of need
	public void GetIndividualGameSettings(){ // called from browser only
		GameSetting gs = new GameSetting(mutedGame, AudioListener.volume, (byte)((AudioListener.pause) ? 2 : 1), Time.timeScale, Cursor.lockState.ToString(), (byte)((Cursor.visible)?2:1));
		browserServices.GameSettings (JsonUtility.ToJson (gs));
	}
	public void DebugMode(int debugVal){
		debugMode = (debugVal == 1);
	}
	public void SetIndividualGameSettings(string data){ // Ad closed?
		//Debug.Log("Set individual game settings");
		GameSetting gs = JsonUtility.FromJson<GameSetting> (data);
		// muted
		mutedGame = gs.m;
		// sound
		if(gs.sp != 0){
			AudioListener.pause = (gs.sp == 2) ? true : false;
		}
		if (gs.sv != -1) {
			AudioListener.volume = gs.sv;
		}
		// timescale
		if(gs.t != -1){
			Time.timeScale = gs.t;
		}

		// pointer
		switch (gs.f) {
		case "Locked": //set focus, hide pointer
			Cursor.lockState = CursorLockMode.Locked;
			//Debug.Log ("Focus: " + state);
			break;
		case "None": //lose focus, display pointer
			Cursor.lockState = CursorLockMode.None;
			//Debug.Log ("Focus: " + state);
			break;
		case "Confined":
			Cursor.lockState = CursorLockMode.Confined;
			//Debug.Log ("Focus: " + state);
			break;
		}
		if (gs.c != 0) {
			Cursor.visible = (gs.c == 2) ? true : false;
		}
	}
	// ######## GAME MANAGEMENT END #######

	// LOG
	private void Log(string logType, string logMsg) {
		if (debugMode) {
			switch (logType) {
			case "warning":
				Debug.LogWarning ("GARTER SDK| "+logMsg);
				break;
			case "error":
				Debug.LogError ("GARTER SDK| "+logMsg);
				break;
			default: Debug.Log ("GARTER SDK| "+logMsg); break;
			}
		}
	}
		
	// -------------------------------- EXTERNAL EVENTS LISTENERS START -------------------------------------------- //
	public enum ExternalListener {
		SdkInitialized,
		CurrencyUpdate,
		EventExternalUpdate,
		ExternalShopButtonPressed,
		ExternalSettingsButtonPressed,
		ReceivedBadges,
		PossibleGameExit,
		RewarededAdState,
		PWAState,

	}

	private Dictionary<ExternalListener, Delegate> externalCallbacksListeners = new Dictionary<ExternalListener, Delegate>(); // callback received from 3-rd party call
	public void AddExternalCbListener<T>(ExternalListener listener, Action<T> cb){
		if (!externalCallbacksListeners.ContainsKey (listener)) {
			externalCallbacksListeners.Add(listener,cb); // add
			if(debugMode) Debug.Log("Adding listener: "+listener.ToString());
		} else { // replace
			externalCallbacksListeners [listener] = cb;
		}
	}
	private void ForwardExternalCb<T>(ExternalListener externalEvent, T cbData){
		if (externalCallbacksListeners.ContainsKey (externalEvent)) {
			foreach(var d in externalCallbacksListeners){
				if (d.Key == externalEvent) {
					d.Value.DynamicInvoke (cbData); break;
				}
			}
		} else {
			Debug.LogWarning ("No function listening for "+externalEvent.ToString()+" found");
			TryRepeatCb (externalEvent, cbData);
		}
	}
	private IEnumerator TryRepeatCb<T>(ExternalListener externalEvent, T cbData){ //For changing notifications / destroying
		yield return new WaitForSeconds (2f);
		Debug.Log ("CB repeat attempt");
		if (externalCallbacksListeners.ContainsKey (externalEvent)) {
			foreach(var d in externalCallbacksListeners){
				if (d.Key == externalEvent) {
					Debug.Log ("New test for "+externalEvent.ToString()+" successful");
					d.Value.DynamicInvoke (cbData); break;
				}
			}
		} else {
			Debug.LogWarning ("New test: No function listening for "+externalEvent.ToString()+" found");
		}
		yield break;
	}

	private enum CachedListener
	{
		_PWAState, // internal PWA State	
		_AdState
	}
	private Dictionary<CachedListener, Delegate> cachedCallbacksListeners = new Dictionary<CachedListener, Delegate>(); // callback received from 3-rd party call
	private void AddCatchCbListener<T>(CachedListener listener, Action<T> cb){
		if (!cachedCallbacksListeners.ContainsKey (listener)) {
			cachedCallbacksListeners.Add(listener,cb); // add
			if(debugMode) Debug.Log("Adding internal listener: "+listener.ToString());
		} else { // replace
			cachedCallbacksListeners [listener] = cb;
		}
	}
	private void ForwardCachedCb<T>(CachedListener internEvent, T cbData){
		if (cachedCallbacksListeners.ContainsKey (internEvent)) {
			foreach(var d in cachedCallbacksListeners){
				if (d.Key == internEvent) {
					d.Value.DynamicInvoke (cbData); break;
				}
			}
		} else {
			Debug.LogWarning ("No function listening for "+internEvent.ToString()+" found");
		}
	}

	// ------------------------------------- start of utilities for Server conunication ------------------------------- //

	// Identification for ServerSDK
	[System.Serializable]
	internal class Identification //get data request
	{
		public string s;
		public uint ig;
		public byte pv;
		public ulong iu;
		public string h;
		public char i;
		public Identification(string sdkVersion, uint projectId, byte projectVersion, ulong idUser, string hash, char interpreter){
			this.s = sdkVersion;
			this.ig = projectId;
			this.pv = projectVersion;
			this.iu = idUser;
			this.h = hash;
			this.i = interpreter;
		}
	}


	// data management - saving in key-value format
	[System.Serializable]
	internal class ServerPureData : Identification
	{
		public string k;
		public ServerPureData(string s, uint ig, byte pv, ulong iu, string h, char i, string key):base(s,ig,pv,iu,h,i){
			this.k = key;
		}
	}

	[System.Serializable]
	internal class ServerPureDataResp : ServerPureData
	{
		public string v;
		public ServerPureDataResp(string s, uint ig, byte pv, ulong iu, string h, char i, string k, string value):base(s,ig,pv,iu,h,i,k){
			this.v = value;
		}
	}
	// ----------------------------------------------- //

	// Wrapper for data sending
	[System.Serializable]
	internal class _ServerObjPost : Identification
	{
		public string d;
		public _ServerObjPost(string s, uint ig, byte pv, ulong iu, string h, char i, string data):base(s,ig,pv,iu,h,i){
			this.d = data; // only key-value data format or full structure
		}
	}

	// Data object for LiteSDK
	[System.Serializable]
	internal class PostUserProgressLite //lite sdk into wrapper
	{
		public decimal[] e; //userEvents
		public decimal lc;
		public Dictionary<string,object> l;
		public float p; // progress
		public byte pv; // project version (protection)
		public PostUserProgressLite(decimal[] eventsVal, decimal localCurrency, Dictionary<string,object> individualSaveData, float progress, byte projectVersion){
			this.e = eventsVal;
			this.lc = localCurrency;
			this.l = individualSaveData;
			this.p = progress;
			this.pv = projectVersion;
		}
	}
	// Data package - initial / shop
	[System.Serializable]
	internal class ReceivedDataLite : PostUserProgressLite //get data
	{
		public float[] ud; //shop sync
		public byte b; // shop sync (data returned)
		public float s; // stars
		public float[] cx; // coinExp
		public ReceivedDataLite(float[] ud, byte ban, float stars, float[] coinsExp, decimal[] e, decimal lc, Dictionary<string,object> l, float p, byte pv):base(e, lc, l, p, pv){
			this.ud = ud;
			this.b = ban;
			this.s = stars;
			this.cx = coinsExp;
		}
	}

	// Data object for fullSDK
	[System.Serializable]
	internal class PostUserProgressFull //full sdk into wrapper
	{
		public decimal[] e;
		public string[] sl;
		public Dictionary<string,object> l;
		public List<string> d;
		public float p;
		public byte pv;
		public PostUserProgressFull(decimal[] eventsVal, string[] slots, Dictionary<string,object> individualSaveData, List<string> dependencies, float progress, byte projectVersion){
			this.e = eventsVal;
			this.sl = slots;
			this.l = individualSaveData;
			this.d = dependencies;
			this.p = progress;
			this.pv = projectVersion;
		}
	}

	// Data package - initial / shop
	[System.Serializable]
	internal class ReceivedDataFull : PostUserProgressFull 
	{
		public int[] i;
		public float[][] iv;
		public float[] ud; //shop sync
		public byte b; // shop sync (data returned)
		public float s;
		public uint uEx;
		public float[] ex;
		public float[] cx;
		public ReceivedDataFull(int[] itemsState, float[][] itemSkillPerformance, float[] userData, byte userBan, float userStars, uint uExperience, float[] eventsExp, float[] coinsExp, decimal[] eventsVal, string[] slots, Dictionary<string,object> individualSaveData, List<string> dependencies, float userProgress, byte projectVersion):base(eventsVal,slots,individualSaveData,dependencies,userProgress, projectVersion){
			this.i = itemsState;
			this.iv = itemSkillPerformance;
			this.ud = userData;
			this.b = userBan;
			this.s = userStars;
			this.uEx = uExperience;
			this.ex = eventsExp;
			this.cx = coinsExp;
		}
	}
		
	[System.Serializable]
	internal class BasicGet
	{
		public string n;
		public float v;
		public string h;
		public BasicGet(string networkId, float version, string hash){
			this.n = networkId;
			this.v = version;
			this.h = hash;
		}
	}

	// Wrapper for data retrieve
	[System.Serializable]
	internal class ServerObjGet
	{
		public string d; //receivedDatalite, receivedDataFull, sync data lite, sync data full
		public string h; // hash
		public byte[] a; //alerts
		public string[] b; //received badges - names
		public string[] bu; // received badge url to big img
		public float[] nb; // need value for next badge (forced synchronization)
		public string[] it; //unlocked items
		public uint[] lp; //leaderboard position
		public ServerObjGet(string data, string hash, byte[] alert, string[] badges, string[] badgesUrl, float[] nextBadgeVal, string[] items, uint[] leaderboard){
			this.d = data;
			this.h = hash;
			this.a = alert; //feature box
			this.b = badges;
			this.bu = badgesUrl;
			this.nb = nextBadgeVal;
			this.it = items;
			this.lp = leaderboard;
		}
	}

	// Wrapper for initial data retrieve
	[System.Serializable]
	internal class ServerObjGetInit : ServerObjGet
	{
		public string[] n; //network
		public byte[] i; //iconsVisibility = [leaderboards,badges,shop,sharing]
		public byte g; //gift
		public ServerObjGetInit(string[] network, byte[] iconsVisibility, byte gift, string data, string hash,byte[] alert, string[] badges, string[] badgesUrl, float[] nextBadgeVal, string[] items, uint[] leaderboard):base(data,hash,alert,badges,badgesUrl,nextBadgeVal,items,leaderboard){
			this.n = network;
			this.i = iconsVisibility;
			this.g = gift;
		}
	}
		
	// Data package - sync
	[System.Serializable]
	internal class SyncDataLite
	{
		public float[] ud; //userData = [coins, diamonds]
		public byte b; // ban
		public float p; // progress
		public float s; // stars
		public decimal c; // lcoal currency
		public SyncDataLite(float[] userData, byte ban, float progress, float stars, decimal currency){
			this.ud = userData;
			this.b = ban;
			this.p = progress;
			this.s = stars;
			this.c = currency;
		}
	}


	[System.Serializable]
	internal class SyncDataFull
	{
		public float[] ud; //userData = [coins, diamonds]
		public byte b; // ban
		public float p; // progress
		public float s; // stars
		public uint uEx;
		public SyncDataFull(float[] userData, byte ban, float progress, byte stars, uint uExperience){
			this.ud = userData;
			this.b = ban;
			this.p = progress;
			this.s = stars;
			this.uEx = uExperience;
		}
	}
		
	// Editor Wrapper
	[System.Serializable]
	internal class EditorProgress
	{
		public WebglPlData g; //gameplayerData
		public ReceivedDataFull rf; //receivedData
		public ReceivedDataLite rl;
		public EditorProgress(WebglPlData gamePlayer, ReceivedDataFull receivedDataFull, ReceivedDataLite receivedDataLite){
			this.g = gamePlayer;
			this.rf = receivedDataFull;
			this.rl = receivedDataLite;
		}
	}

	// GamePlayer data package
	[System.Serializable]
	internal class WebglPlData //browser
	{
		public ulong i; //userId
		public string[] u; //user = [nick, lang, country]
		public string pu; //URL for download  user img
		public float[] ud; // [coins, diamonds]
		public float p; // userProgress
		public byte b; // ban
		public float s; // user stars
		public byte gm; //individualGameMode"// limitations
		public byte[] sf; //saveFrequency
		public string h; //hash
		public string[] sc; //starColors = [#f000, ...]
		public string[] r; //user ranks = [silver, gold,...]
		public byte ol; // outgoing links
		public string rf; //referrer
		public string su; //server url
		public string[] l; // logo names
		public string bn; // browser name
		public byte a; // activityService
		public bool ra; //rewarded ads
		public string na; // network protection
		public string ni; // network unique identificator
		public string pw; // progressiveWebApp
		public WebglPlData(uint userId, string[] user, string photoUrl, float[] userData, float userProgress, byte ban, float userStars, byte individualGameMode, byte[] saveFrequency, string hash, string[] starsColors, string[] ranks, string outgoingLink, byte outgoingLinks, string referrer, string serverUrl, string[] logoNames, string browserName, byte activityService, bool rewardedAdsEnabled, string networkAuthentication, string networkUniqueIdentificator, string progressiveWebAppState){
			this.i = userId;
			this.u = user;
			this.pu = photoUrl; 
			this.ud = userData;
			this.p = userProgress;
			this.b = ban;
			this.s = userStars;
			this.gm = individualGameMode;
			this.sf = saveFrequency;
			this.h = hash;
			this.sc = starsColors;
			this.r = ranks;
			this.ol = outgoingLinks;
			this.rf = referrer;
			this.su = serverUrl;
			this.l = logoNames;
			this.bn = browserName;
			this.a = activityService;
			this.ra = rewardedAdsEnabled;
			this.na = networkAuthentication;
			this.ni = networkUniqueIdentificator;
			this.pw = progressiveWebAppState;
		}
	}
		
	// module opening config info
	[System.Serializable]
	internal class OpenWindowConf
	{
		public string w;
		public byte s;
		public OpenWindowConf(string window, byte sync){
			this.w = window;
			this.s = sync;
		}
	}

	[System.Serializable]
	private class FeatureBarIcons
	{
		public byte[] visibility;
		public byte[] alerts;
	}
}