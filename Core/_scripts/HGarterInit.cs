using UnityEngine;
using System.Collections.Generic;

[HideInInspector]
public class HGarterInit : MonoBehaviour {

	//Awake is running on first load only - nefunguje
	public enum Active {
		Yes, No
	}
	public Active active;
	public Active enabledProtection;

	public uint projectId;
	public enum ProjectVersion {
		V1, V2, V3, V4, V5
	}
	public ProjectVersion projectVersion;

	public enum Platform {
		Webgl
	}
	public Platform platform;

	public enum MultiplayerGame {
		No, Yes
	}
	public MultiplayerGame multiplayer;
	public Active garterNetwork;

	public Sdk sdk;
	public enum Sdk {
		Basic,
		Lite,
		Full
	}

	public Active ingameCurrency;

	// selectors
	public enum EnabledDisabled {
		Enabled, Disabled
	}
	public enum BtnVisibility {
		Displayed, Hidden
	}
		
	// group classes
	[System.Serializable]
	public class Features {
		public EnabledDisabled notifications;
	}
	public Features gamearterFeatures;

	[System.Serializable]
	public class FeatureBox {
		public BtnVisibility settingsButton;
	}
	[System.Serializable]
	public class AdvancedFeatureBox : FeatureBox {
		public BtnVisibility shopButton;
	}
	public AdvancedFeatureBox advancedFeatureBox;
	public FeatureBox featureBox;

	// progress selection
	public enum ProgressMode
	{
		Sdk,
		Individual,
		None
	}
	[System.Serializable]
	public class ProgressModule
	{
		public ProgressMode mode;
		public BtnVisibility sdkProgressBar;

	}
	public ProgressModule progressModule;
	// end of progress section


	[Range(0.0f, 1.0f)]
	public float minimumTimescale;

	public enum AnalyticsMode {
		Automatic, Manual, Off
	}
	public AnalyticsMode analyticsMode;
	public EnabledDisabled autoSaving;

	public enum Trend {
		Increasing, Decreasing, Undefined
	}

	[System.Serializable]
	public class Events
	{
		public string groupName;
		public EventConfig[] groupEvents;

		[System.Serializable]
		public class EventConfig
		{
			public string nameId;
			public float initialValue;
			public Trend trend;
		}
	}
	public Events[] events;

	//initial items in a stack
	private List<string> stackDataList = new List<string> ();


	[System.Serializable]
	public class InitialProperties
	{
		public string parameterName;
		public float parameterValue;
	}


	[System.Serializable]
	public class Property 
	{
		public Items[] items;
		public Accessories[] accessories;
	}
	public Property property;


	// items
	[System.Serializable]
	public class Items 
	{
		public string nameId;
		public enum InitialState {
			selected, bought, unlocked, locked
		}
		public InitialState initialState;
		public string initialAccessories;
		public List<InitialProperties> initialProperties;
	}
		
	// accessories
	[System.Serializable]
	public class Accessories 
	{
		public string nameId;
		public enum InitialStateAcs {
			bought, unlocked, locked
		}
		public InitialStateAcs initialStateAsc;
		public List<InitialProperties> initialProperties;
	}
	//public List<Accessories> accessories = new List<Accessories>();

	public string individual;
			
	void Awake() {
		if (Garter.I.IsFirstLoad () && active == Active.Yes) { //Initialize
			Verification ();
		} else if(active == Active.Yes){ //update GUI
			Garter.I.Initialize(null);
		}
	}
		

	private void Verification()
	{
		#if UNITY_EDITOR && UNITY_WEBGL
		if (projectId == 0) Debug.LogError ("projectId cannot be 0");
		#endif

		object[] output = null;

		char interpreter = 'B';
		switch (sdk) {
		case Sdk.Full: interpreter = 'F'; break;
		case Sdk.Lite: interpreter = 'L'; break;
		}
			
		byte projectVersionNum = 1;
		switch (projectVersion) {
		case ProjectVersion.V2: projectVersionNum = 2; break;
		case ProjectVersion.V3: projectVersionNum = 3; break;
		case ProjectVersion.V4: projectVersionNum = 4; break;
		case ProjectVersion.V5: projectVersionNum = 5; break;
		}
			
		if (!interpreter.Equals ('B')) {
			// ----- Events ----- //
			// Create arrays of Event properties
			int eventsLength = 0;
			int groupsLength = events.Length;
			for (int i = 0; i < groupsLength; i++) {
				eventsLength += events [i].groupEvents.Length;
			}
		
			string[] eventsNames = new string[eventsLength];
			decimal[] eInitValue = new decimal[eventsLength];
			byte[] eIncreasingTrend = new byte[eventsLength];

			int eventIndex = 0;
			for (int i = 0; i < groupsLength; i++) {
				int eLength = events [i].groupEvents.Length;
				for (int j = 0; j < eLength; j++) {
					eventsNames [eventIndex] = events [i].groupEvents [j].nameId;
					#if UNITY_EDITOR && UNITY_WEBGL
					if(events [i].groupEvents [j].nameId == ""){
						Debug.LogError("nameId of event with index "+j+" inside group "+events [i].groupName+" cannot be empty string");
					}
					#endif
					eInitValue [eventIndex] = (decimal)events [i].groupEvents [j].initialValue;
					switch (events [i].groupEvents [j].trend) {
					case Trend.Increasing:
						eIncreasingTrend [eventIndex] = 0;
						break;
					case Trend.Decreasing:
						eIncreasingTrend [eventIndex] = 1;
						break;
					case Trend.Undefined:
						eIncreasingTrend [eventIndex] = 2;
						break;
					}
					eventIndex++;
				}
			}
				
			// check events for duplicity
			#if UNITY_EDITOR && UNITY_WEBGL
			string gameEventsC = "game events (" + eventsLength + "): ";
			HashSet<string> gameEventsDuplicity = new HashSet<string> (); //check itemNames for duplicity
			for (int i = 0; i < eventsLength; i++) {
				gameEventsDuplicity.Add (eventsNames [i]); 
				gameEventsC += eventsNames [i] + ", ";
			}
			if (eventsLength != gameEventsDuplicity.Count) {
				string names = "";
				HashSet<string> itemsSeen = new HashSet<string> ();
				HashSet<string> itemsYielded = new HashSet<string> ();
				foreach (string item in eventsNames) {
					if (!itemsSeen.Add (item)) {
						if (itemsYielded.Add (item)) {
							names += "*"+item+"* ";
						}
					}
				}
				Debug.LogError ("GameArter | Found duplicated events: "+ names+"| Please, remove duplicities.");
			}
			#endif

			// ----- items ----- //
			int itemsL = property.items.Length;
			int accessoriesL = property.accessories.Length;
			int itemsChildrens = itemsL + accessoriesL;
			// variables
			string[] itemNames = new string[itemsChildrens]; //itemNames: utem0, item1 ...
			int[] itemsState = new int[itemsChildrens]; //itemsStates: -1 = locked, 0 = unlocked, 1 = upgrade 1 ...
			string[][] itemSkillName = new string[itemsChildrens][]; //propertySkillName: ["attack","defense"] - undefined number of skills -> jagged array
			float[][] itemSkillPerformance = new float[itemsChildrens][]; //propertySkillPerformance: [100,20]
			List<string> dependencies = new List<string> (); //initial dependencies item1 + item 7 = e.g. weapon1 + laser3

			if (interpreter.Equals('F')) {
				#if UNITY_EDITOR && UNITY_WEBGL
				// extract taken data to a console + check for duplicity
				string propertyDataC = "Items (" + itemsChildrens + "): ";
				#endif

				// ----- items -----
				for (int i = 0; i < itemsL; i++) { // zapis itemu
					
					string itemName = property.items [i].nameId;
					// name
					itemNames [i] = itemName; // set itemName
					// state
					itemsState [i] = getItemState (property.items [i].initialState); // set itemState

					if (property.items [i].initialState == Items.InitialState.selected) stackDataList.Add (itemName); // add to stack data, if state is selected

					// dependencies
					string dependency = property.items [i].initialAccessories; // nastav defaultne
					if (!string.IsNullOrEmpty(dependency)) dependencies.Add (itemName+"#"+dependency);

					#if UNITY_EDITOR && UNITY_WEBGL
					propertyDataC += "|| " + itemName + ": state=" + itemsState [i];
					propertyDataC += ", dependencies: " + dependency + ", properties: "; //searching according to top item (index 0 after parsing)
					#endif

					// initial properties - check and add Property
					int propertiesChild = property.items [i].initialProperties.Count;
					List<InitialProperties> bpData = property.items [i].initialProperties;
					itemSkillName [i] = new string[propertiesChild];
					itemSkillPerformance [i] = new float[propertiesChild];
					for (var k = 0; k < propertiesChild; k++) {
						InitialProperties bp = bpData [k];
						itemSkillName [i] [k] = bp.parameterName;
						itemSkillPerformance [i] [k] = bp.parameterValue;

						#if UNITY_EDITOR && UNITY_WEBGL
						if (itemSkillName [i] [k] == "") {
							if(itemName == ""){
								Debug.LogError ("Item named '' is inside items section. Please, fill its name.");
							} else {
								Debug.LogError ("GameArter | Property of *" + itemName + "* is empty. Please, fill it.");
							}
						}
						propertyDataC += itemSkillName [i] [k] + "=" + itemSkillPerformance [i] [k] + " ";
						#endif
					}
				} // end of item for loop



				// ----- accessories -----
				for (int i = 0; i < accessoriesL; i++) { // zapis accessories
					int indexY = itemsL + i;
					// name
					string accessoryName = property.accessories [i].nameId;
					itemNames [indexY] =  accessoryName;
					// state
					itemsState [indexY] = getAccessoryState (property.accessories [i].initialStateAsc);

					#if UNITY_EDITOR && UNITY_WEBGL
					propertyDataC += "|| " + itemNames [indexY] + ": state=" + itemsState [indexY];
					#endif

					// properties
					int propertiesChild = property.accessories [i].initialProperties.Count;
					List<InitialProperties> bpData = property.accessories [i].initialProperties;
					itemSkillName [indexY] = new string[propertiesChild];
					itemSkillPerformance [indexY] = new float[propertiesChild];
					for (var k = 0; k < propertiesChild; k++) {
						InitialProperties bp = bpData [k];
						itemSkillName [indexY] [k] = bp.parameterName;
						itemSkillPerformance [indexY] [k] = bp.parameterValue;

						#if UNITY_EDITOR && UNITY_WEBGL
						if (itemSkillName [indexY] [k] == "") {
							if(accessoryName == ""){
								Debug.LogError ("Item named '' is inside accessories section. Please, fill the name.");
							} else {
								Debug.LogError ("GameArter | Property of *" + accessoryName + "* is empty. Please, fill it.");
							}
						}
						propertyDataC += itemSkillName [indexY] [k] + "=" + itemSkillPerformance [indexY] [k] + " ";
						#endif
					}
				} // end of accessories loop


				#if UNITY_EDITOR && UNITY_WEBGL
				//check item name for duplicity
				if (stackDataList.Count == 0) {
					Debug.LogWarning ("GameArter | No items inside stack. Is this right?");
				}
					
				//Debug.Log (propertyDataC);
				#endif
			} // end of full sdk


			// create stack data array
			string[] stackData = new string[0];
			int stackDataListChilds = stackDataList.Count;
			if (stackDataListChilds > 0) {
				stackData = new string[stackDataListChilds];
				for (int i = 0; i < stackDataListChilds; i++) {
					stackData [i] = stackDataList [i];
				}
			}
				
			// settings for sdk
			byte[] featureButtons = GetFeatureButtons();
			bool[] featuresNotification = new bool[] {gamearterFeatures.notifications == EnabledDisabled.Enabled};

			bool progressBarVisibility = false;
			if (progressModule.mode != ProgressMode.None) progressBarVisibility = (progressModule.sdkProgressBar == BtnVisibility.Displayed);

			if (enabledProtection == Active.No) Debug.LogWarning ("GARTER | Steal protection is beeing disabled");


			output = new object[]{
				projectId, //0
				projectVersionNum, //1
				interpreter, //2
				active == Active.Yes, //3
				minimumTimescale, //4
				enabledProtection == Active.Yes, //5
				multiplayer == MultiplayerGame.Yes, //6
				analyticsMode.ToString().ToLower(), //7
				eventsNames, //8
				eInitValue, //9
				eIncreasingTrend, //10
				itemNames, //11
				itemsState, //12
				itemSkillName, //13
				itemSkillPerformance, //14
				dependencies, //15
				stackData, //16
				featureButtons, //17
				featuresNotification, //18
				progressBarVisibility, //19
				individual, //20
				autoSaving == EnabledDisabled.Enabled, //21
				ingameCurrency == Active.Yes, //22
				platform //23
			};
		} else { //Basic SDK
			output = new object[] {
				projectId, //0
				projectVersionNum, //1
				interpreter, //2
				active == Active.Yes, //3
				minimumTimescale, //4
				enabledProtection == Active.Yes, //5
				multiplayer == MultiplayerGame.Yes, //6
				analyticsMode.ToString().ToLower(), //7
				GetFeatureButtons(), //8 display settings btn in featureBar
				platform //9
			};
		}
		Garter.I.Initialize (output);
	}

	private byte[] GetFeatureButtons(){
		byte[] featureButtons = null;
		switch (sdk) {
		case Sdk.Basic: // settings btn only
			featureButtons = new byte[]{
				(byte)((featureBox.settingsButton == BtnVisibility.Displayed) ? 1 : 0)
			};
			break;
		case Sdk.Lite: // obe z initu
			featureButtons = new byte[] {
				(byte)((advancedFeatureBox.shopButton == BtnVisibility.Displayed) ? 1 : 0),
				(byte)((advancedFeatureBox.settingsButton == BtnVisibility.Displayed) ? 1 : 0)
			};
			break;
		case Sdk.Full:// shop ze serveru
			featureButtons = new byte[]{(byte)((featureBox.settingsButton == BtnVisibility.Displayed) ? 1 : 0)};
			break;
		}
		return featureButtons;
	}
		
	private int getItemState(Items.InitialState state){
		int value = 0;
		switch (state) { 
		case Items.InitialState.locked:
			value = -2;
			break;
		case Items.InitialState.unlocked:
			value = -1;
			break;
		case Items.InitialState.bought:
			value = 0;
			break;
		}
		return value;
	}
	private int getAccessoryState(Accessories.InitialStateAcs state){
		int value = 0;
		switch (state) { 
		case Accessories.InitialStateAcs.locked:
			value = -2;
			break;
		case Accessories.InitialStateAcs.unlocked:
			value = -1;
			break;
		case Accessories.InitialStateAcs.bought:
			value = 0;
			break;
		}
		return value;
	}
}