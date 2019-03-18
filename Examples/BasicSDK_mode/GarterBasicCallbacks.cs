using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarterBasicCallbacks : MonoBehaviour {

	private GameObject buttonInstallAsApp;

	void Awake(){
		// ADD LISTENERS FOR EXTERNAL EVENTS
		Garter.I.AddExternalCbListener<GarterWWW>(Garter.ExternalListener.SdkInitialized, SdkInitialized);
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.ExternalSettingsButtonPressed, SettingsButtonPressed);
		Garter.I.AddExternalCbListener<string> (Garter.ExternalListener.ActiveTabMonitor, ActiveTabMonitor);

		buttonInstallAsApp = GameObject.Find ("3.11 Install As Web App"); // Save button
		ProgressiveWebAppState (Garter.I.GetStatePWA()); // Hide Button
		Garter.I.AddExternalCbListener<string>(Garter.ExternalListener.PWAState, ProgressiveWebAppState); // Install listener

		// Dont destroy on load - variables and functions will be available in every scene
		DontDestroyOnLoad(transform.gameObject);
	}

	private void SdkInitialized(GarterWWW data){
		Debug.Log ("SDK initialized - Garter calls are available.");
		// set user data (nickname etc) and load game
	}

	private void RewardedAdCallback(string state){
		Debug.Log ("rewarded-ad callback| "+state);
		// success - ad has been succesfullz displayed
		// no-ad - we found no ad for the user
		// fail - some problem in displaying ad
	}

	private void SettingsButtonPressed(string emptyString = null){
		Debug.Log ("GARTER CALLBACK - open settings");
		// Called on pressing "Settings" button inserted in "GameArter_Features" prefab
		// visibility of this button is customizable via GameArter_Initialize
	}

	private void ProgressiveWebAppState(string state){
		Debug.Log ("Progressive Web App State: "+state);
		if (buttonInstallAsApp != null) {
			if (state == "enabled") {
				// display button "install as app"
				buttonInstallAsApp.SetActive(true);
			} else if(state == "disabled"){
				// hide button "install as app"
				buttonInstallAsApp.SetActive(false);
			}
		}
	}

	private void ActiveTabMonitor(string state){
		Debug.Log ("Browser tab with game is " + state);
	}
}
