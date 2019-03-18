using UnityEngine;

public class GarterFullCallbacks : MonoBehaviour {

	void Awake(){
		// ADD LISTENERS FOR EXTERNAL EVENTS
		Garter.I.AddExternalCbListener<GarterWWW>(Garter.ExternalListener.SdkInitialized, SdkInitialized);
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.ExternalSettingsButtonPressed, SettingsButtonPressed);
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.EventExternalUpdate, EventUpdateViaShop);
		Garter.I.AddExternalCbListener<string[]>(Garter.ExternalListener.ReceivedBadges, BadgeReceived);
		Garter.I.AddExternalCbListener<float>(Garter.ExternalListener.CurrencyUpdate, CurrencyUpdateInOwnGui);

		// Attach this class to Dont destroy on load - variables and functions will be available in every scene
		DontDestroyOnLoad(transform.gameObject);
	}
		
	private void SdkInitialized(GarterWWW www){
		Debug.Log ("Full SDK Initialized - all data, features and calls are available");
		Debug.Log ("UserNick: "+Garter.I.UserNick());
	}
		
	private void SettingsButtonPressed(string emptyString = null){
		Debug.Log ("GARTER CALLBACK - open settings");
	}

	private void RewardedAdCallback(string state){
		Debug.Log ("rewarded-ad| "+state);
		// success - ad has been succesfullz displayed
		// no-ad - we found no ad for the user
		// fail - some problem in displaying ad
	}

	private void EventUpdateViaShop(string emptyString = null){
		// In the external shop/exange, users can use their money 
		// for changing value of currency (see CurrencyUpdate) as well as events
		// with "undefined" trend. These event occurs, when any event with 
		// undefined trend was changed via in-game shop (by purchase).
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

	// external change in currency state - purchase in a shop, reward from a badge...
	// gui of gamearter prefabs is being updated automatically
	// use this for updates of currency value in own GUI
	private void CurrencyUpdateInOwnGui(float currentValue){
		Debug.Log ("new currency value: " + currentValue);
	}
}
