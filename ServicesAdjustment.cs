using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Services adjustment. - adjust game behavior during opened gamearter modules - https://developers.gamearter.com/docs/gamearter-modules
/// </summary>
public class ServicesAdjustment {

	private CursorLockMode cachedCursorLock;
	private float cachedVolume, cachedTimeScale; // default sound volume and game timescale at the beggining of the game
	private bool cachedValues = false;
	public void CacheGameSettings(){
		if (!cachedValues) {
			cachedCursorLock = Cursor.lockState;
			cachedVolume = AudioListener.volume;
			cachedTimeScale = Time.timeScale;
			cachedValues = true;
		}
	}
		
	public void ModuleWindowOpened(string moduleName){

		CacheGameSettings (); // cache current state - game will be returned to this state after closing an ad (=mute game)

		float minimumTimeScale = Garter.I.GetMinimumTimeScale ();

		//1) mute audio
		try{
			AudioListener.volume = 0f;
			if (minimumTimeScale == 0) AudioListener.pause = true;
		} catch {
			Debug.LogWarning ("GarterError | Audio Listener setting");
		}


		//2) pause game
		Time.timeScale = minimumTimeScale;
		
		//3) display pointer
		if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
		if (!Cursor.visible) Cursor.visible = true;
		
		/* individual customization for certain modules, if needed (available moduleNames: https://developers.gamearter.com/docs/gamearter-modules)
		if (moduleName == "ad") ...*/
	}
		
	public void ModuleWindowClosed(string moduleName){
		cachedValues = false;

		//1) unpause game
		Time.timeScale = cachedTimeScale;

		//2) unmute game
		try{
			AudioListener.volume = cachedVolume;
			if (Garter.I.GetMinimumTimeScale () == 0) AudioListener.pause = false;
		} catch {
			Debug.LogWarning ("GarterError | Audio Listener setting");
		}
			
		//3) pointer management
		Cursor.lockState = cachedCursorLock;

		/* individual customization for certain modules, if needed (available moduleNames: https://developers.gamearter.com/docs/gamearter-modules)
		if (moduleName == "ad") ...*/
	}
		
	public void ModuleWindowChanged(string currentModule){ // switching between modules (optional adjustment)
		Debug.Log ("Currently opened module: "+currentModule);
		// available module names: https://developers.gamearter.com/docs/gamearter-modules
	}
}