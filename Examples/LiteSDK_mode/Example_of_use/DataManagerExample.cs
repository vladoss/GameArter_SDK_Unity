using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataManagerExample : MonoBehaviour {

	void Awake(){
		// ADD LISTENERS FOR EXTERNAL EVENTS
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.SdkInitialized, GameArterSDKInitialized);
		Garter.I.AddExternalCbListener<decimal>(Garter.ExternalListener.CurrencyUpdate, CurrencyUpdate);
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.EventExternalUpdate, EventUpdateViaShop);
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.ExternalShopButtonPressed, ShopButtonPressed);
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.ExternalSettingsButtonPressed, SettingsButtonPressed);
		Garter.I.AddExternalCbListener<string[]>(Garter.ExternalListener.ReceivedBadges, BadgeReceived);
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.PossibleGameExit, PossibleGameExit);
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.RewarededAdState, RewardedAdCallback);

		// Dont destroy on load - variables and functions will be available in every scene
		DontDestroyOnLoad(transform.gameObject);
	}

	// SDK INITITALIZATION
	private void GameArterSDKInitialized(string sdkError)
	{
		if (string.IsNullOrEmpty (sdkError)) {

			// User data are available
			Debug.Log("UserNick: " + Garter.I.UserNick());
			Debug.Log("UserLanguage: " + Garter.I.UserLang());
			Debug.Log("UserCountry: " + Garter.I.UserCountry());
			Debug.Log("LoggedUser?: " + Garter.I.IsLoggedUser());
			Debug.Log("User Image:" + Garter.I.UserImage());
			Debug.Log ("Game progress in game: " + Garter.I.UserProgress ());
			Debug.Log ("User's game funds: " + Garter.I.LocalCurrency ());

			// if your game is multiplayer, since now photon id and ver is available
			if (Garter.I.GetMultiplayerNetwork() != null)
			{
				Debug.Log("PHOTON ID HERE");
				//string photonId = Garter.I.GetMultiplayerNetwork () [0]; - uncomment for using
				//string photonVersion = Garter.I.GetMultiplayerNetwork () [1]; - uncomment for using
				Debug.Log("Network authentication | " + Garter.I.NetworkAuthentication() + " / " + Garter.I.NetworkUniqueUserId());
			}
				
			// get value of a saved key
			GetData();

		} else {
			Debug.LogError("ERR: " + sdkError);
			new HGarterGui().IlustrateBrowserBox("error", "Ops, something went wrong. We apologize");
		}

		CurrencyUpdate (Garter.I.LocalCurrency ());
	}

	public void GetData(){
		string myKeyData = Garter.I.GetData<string>("my-key");
		Debug.Log ("my-key: "+myKeyData);

		int myNumData = Garter.I.GetData<int>("my-num");
		Debug.Log ("my-num: "+myNumData);

		string sdkv1data = Garter.I.GetData<string>("default");
		Debug.Log ("sdkv1data: "+sdkv1data);

		/*string myComplexData = Garter.I.GetData<string>("complexData",(error,value) => {
			if(!string.IsNullOrEmpty(error)){
				Debug.Log ("myComplexData callback - error: " + error);
			} else {
				Debug.Log ("myComplexData callback - data: " + Garter.I.FromJson<JsonStructure>(value));
			}
		});
		Debug.Log ("myComplexData: "+Garter.I.FromJson<JsonStructure>(myComplexData));*/

		// INSTEAD OF THIS, USE THE WAY DESCRIBED ABOVE
		JsonStructure myComplexData = Garter.I.GetData<JsonStructure>("complexData",(error,value) => {
			if(!string.IsNullOrEmpty(error)){
				Debug.Log ("myComplexData callback - error: " + error);
			} else {
				Debug.Log ("myComplexData callback - data: " + value);
			}
		});
		Debug.Log ("myComplexData: "+myComplexData);
	}

	public void PostData(){
		// post data without callback
		Garter.I.PostData<string>("my-key", "Hello World");

		// post data with callback
		Garter.I.PostData<int>("my-num", 6, (error, response) => {
			if (!string.IsNullOrEmpty (error)) {
				Debug.LogWarning (error);
			} else {
				Debug.Log (response);
			}
		});
			
		// Own data type (defined by Class)
		JsonStructure complexData = new JsonStructure (someStringData, someIntegerData, someFloatData, new JsonStructure.MyTypeData (someListType, someArrayType, someJaggedArrayType));
		// WARNING - NEED TO CONVERT TO STRING
		// REASON: THIS WOULD THROW ERROR FOR USERS PLAYING AS GUESTS. THE FORMAT IS BROKEN ONCE IS SAVED IN PLAYEPREFS. THEREFORE WE RECOMMEND TO USE CONVERSION TO STRING FOR ALL.
		string complexDataString = Garter.I.ToJson(complexData); // own data types are not allowed for guests. Need to convert the data to string format
		Garter.I.PostData<string>("complexData", complexDataString, (error, response) => {
			if (!string.IsNullOrEmpty (error)) {
				Debug.LogWarning (error);
			} else {
				Debug.Log (response);
			}
		});
	}


	// YOUR GAME SURELY CONTAIN SOME USER'S PROGRESS DATA NEED TO BE SAVED
	// FUNCTIONAL EVENTS LIKE KILLS SHOULD BE SAVED AS EVENTS
	// ALL OTHER DATA YOU CAN SAVE IN YOUR OWN DEFINED STRUCTURE. SEE THE EXAMPLE BELOW. ALL DATA TYPES ARE SUPPORTED.
	public string someStringData = "Some String Data";
	public int someIntegerData = 8;
	public float someFloatData = 1.3f;
	public List<byte> someListType = new List<byte>(5) { 0, 1, 2, 3, 4 };
	public bool[] someArrayType = new bool[] { true, false };
	public ulong[][] someJaggedArrayType = new ulong[2][] { new ulong[] { 0, 1, 2, 3 }, new ulong[] { 4, 5, 6 } }; // array composted of arrays

    // DEFINE DATA STRUCTURE FORMAT FOR POSTING THE DATA TO SERVER
    [System.Serializable]
    internal class JsonStructure
    {
        public string someStringData;
        public int someIntegerData;
        public float someFloatData;
        public MyTypeData someOwnTypeData;
        public JsonStructure(string stringData, int integerData, float floatData, MyTypeData ownTypeData)
        {
            this.someStringData = stringData;
            this.someIntegerData = integerData;
            this.someFloatData = floatData;
            this.someOwnTypeData = ownTypeData;
        }

        [System.Serializable]
        internal class MyTypeData
        { // own data type
            public List<byte> someListType;
            public bool[] someArrayType;
            public ulong[][] someJaggedArrayType;
            public MyTypeData(List<byte> list, bool[] array, ulong[][] jaggedArray)
            {
                this.someListType = list;
                this.someArrayType = array;
                this.someJaggedArrayType = jaggedArray;
            }
        }
    }

    // FUNCTION FOR POSTING THE DATA TO SERVER
	public void PostDataInMeantime(string emptyString = null) // potreba prepracovat
    {
        string dataToBeSaved = Garter.I.ToJson(new JsonStructure(someStringData, someIntegerData, someFloatData, new JsonStructure.MyTypeData(someListType, someArrayType, someJaggedArrayType)));
        Garter.I.SetIndividualGameData<string>("default",dataToBeSaved);
    }
		
    // overwrite default values by received values
    private void SaveToVariables(string json)
    {
        //Debug.Log ("Save user data from server to variables" + json);
        // parse json data
        JsonStructure dataClass = Garter.I.FromJson<JsonStructure>(json);
        // overwrite default data by parsed data
        someStringData = dataClass.someStringData;
        someIntegerData = dataClass.someIntegerData;
        someFloatData = dataClass.someFloatData;
        // myTypeData
        someListType = dataClass.someOwnTypeData.someListType;
        someArrayType = dataClass.someOwnTypeData.someArrayType;
        someJaggedArrayType = dataClass.someOwnTypeData.someJaggedArrayType;
    }

	/// GENERAL FUNCTION FOR UPDATING DISPLAYED CURRENCY - DISPLAYES RECEIVED "currencyValue"
    private void CurrencyUpdate(decimal currencyValue)
    {
		// Value of currency was changed out of the game.
		// e.g. via currency purchase in exchange
		// currency rewards was part of other rreward (badge, leaderboard...)
		Debug.Log("SET User's game currency value to: " + currencyValue);
       
		// Update currency value displayed in the UI
		try
        {
			GameObject userFunds = GameObject.Find("UserFunds");
			Text userFundsTxt = userFunds.GetComponent<Text>();
			userFundsTxt.text = "User funds: " + currencyValue;
        }
        catch
        {
            Debug.Log("Object with currency was not found");
        }

    }

	private void EventUpdateViaShop(string emptyString = null){
		// In the external shop/exange, users can use their money 
		// for changing value of currency (see CurrencyUpdate) as well as events
		// with "undefined" trend. These event occurs, when any event with 
		// undefined trend was changed via in-game shop (by purchase).
		Debug.Log ("Value of some event with undefined trend was updated via shop - update UI, if needed");
	}

	private void ShopButtonPressed(string emptyString = null){ // open your shop (SDK lite) (click listener in GameArter_Features prefab)
		Garter.I.OpenSdkWindow ("exchange"); // example - open either this for currency exchange or your in-game shop
		// Called on pressing "Shop" button inserted in "GameArter_Features" prefab;
		// visibility of this button is customizable via GameArter_Initialize
	}

	private void SettingsButtonPressed(string emptyString = null){
		Debug.Log ("GARTER CALLBACK - open settings");
		// Called on pressing "Settings" button inserted in "GameArter_Features" prefab
		// visibility of this button is customizable via GameArter_Initialize
	}
	// received badges callback - useful for a case you want to display a special screen or image
	private void BadgeReceived(string[] badgeName){
		// Badges (Achievements) are distributed automatically in realtime
		// on basis of updating defined game Events.
		// In that moment, list of received badges is sent to this functions.
		// By default, a user see a small notification about received badge 
		// in top left corner and alert icon on badges button (part of "GameArter_Features" prefab).
		// This function can be used if you wish to display a bigger image or any action you want.
		for (byte i = 0; i < badgeName.Length; i++) Debug.Log ("Badge received | "+badgeName[i]);
	}

	private void PossibleGameExit(string emptyString = null){
		//save data - risk, a user will leave the game
	}

	private void RewardedAdCallback(string state){
		Debug.Log ("rewarded-ad callback| "+state);
		// success - ad has been succesfullz displayed
		// no-ad - we found no ad for the user
		// fail - some problem in displaying ad
	}
}