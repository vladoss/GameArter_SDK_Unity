using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

[ExecuteInEditMode]
[CustomEditor(typeof(HGarterInit))]
public class HGarterInitEditor : Editor 
{
	[MenuItem("Tools/GameArter/web")]
	static void Web(){
		Application.OpenURL ("https://www.gamearter.com");
	}

	[MenuItem("Tools/GameArter/documentation")]
	static void Documentation(){
		Application.OpenURL ("https://developers.gamearter.com/docs/unity/sdk-overview.php");
	}

	private uint projectId;
	private byte projectVer = 0;

	public override void OnInspectorGUI()
	{
		HGarterInit init = (HGarterInit)target;

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.HelpBox ("\nDO NOT FILL THIS IN PLAY MODE\n" +
			"If you used prefab, you must save filled data by pressing APPLY button in top right corner.\n" +
			"\nAny issue or do you need help with anything? -> vladimir@gamearter.com\n", MessageType.Info);
		EditorGUILayout.EndHorizontal();

		serializedObject.Update();
		EditorGUILayout.PrefixLabel("");

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Documentation:");
		if(GUILayout.Button("Visit GameArter knowleadge base"))
		{
			Application.OpenURL("https://developers.gamearter.com/docs/unity/sdk-overview.php");
		}
		EditorGUILayout.EndHorizontal();

		//active / deactive sdk
		EditorGUILayout.BeginHorizontal ();
		init.active = (HGarterInit.Active)EditorGUILayout.EnumPopup ("Active?", (HGarterInit.Active)init.active);
		EditorGUILayout.EndHorizontal();

		int activeVal = (init.active == HGarterInit.Active.Yes ? 1 : 0);
		using (var disactive = new EditorGUILayout.FadeGroupScope (activeVal)) {
			if (!disactive.visible) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.HelpBox ("GameArter SDK is being deactivated. Switch Active to Yes.", MessageType.Warning);
				EditorGUILayout.EndHorizontal ();
			} else {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Project Id");
				init.projectId = (uint)EditorGUILayout.IntField((int)init.projectId);
				projectId = init.projectId;
				if(GUILayout.Button("Get Id")) Application.OpenURL ("https://developers.gamearter.com/projects");
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal ();
				switch(init.projectVersion){
				case HGarterInit.ProjectVersion.V1:
					projectVer = 1;
					break;
				case HGarterInit.ProjectVersion.V2:
					projectVer = 2;
					break;
				case HGarterInit.ProjectVersion.V3:
					projectVer = 3;
					break;
				case HGarterInit.ProjectVersion.V4:
					projectVer = 4;
					break;
				case HGarterInit.ProjectVersion.V5:
					projectVer = 5;
					break;
				}
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("projectVersion"), true);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("platform"), true);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.PrefixLabel("");

				EditorGUILayout.BeginHorizontal ();
				init.enabledProtection = (HGarterInit.Active)EditorGUILayout.EnumPopup ("Enabled Steal Protection", (HGarterInit.Active)init.enabledProtection);
				EditorGUILayout.EndHorizontal();
				if (init.enabledProtection == HGarterInit.Active.No) {
					EditorGUILayout.HelpBox ("Steal protection is not being active.", MessageType.Warning);
				}
					
				EditorGUILayout.BeginHorizontal ();
				init.multiplayer = (HGarterInit.MultiplayerGame)EditorGUILayout.EnumPopup ("Multiplayer Game", (HGarterInit.MultiplayerGame)init.multiplayer);
				EditorGUILayout.EndHorizontal();
				bool multiplayer = (init.multiplayer == HGarterInit.MultiplayerGame.Yes ? true : false);
				using (var multiplayerGame = new EditorGUILayout.FadeGroupScope ((multiplayer)? 1 : 0)) {
					if (multiplayerGame.visible) {
						EditorGUILayout.BeginHorizontal ();
						init.garterNetwork = (HGarterInit.Active)EditorGUILayout.EnumPopup ("GameArter photonId", (HGarterInit.Active)init.garterNetwork);
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.PrefixLabel("");
					}
				}

				multiplayer = ((multiplayer) ? (init.garterNetwork == HGarterInit.Active.Yes) : false);
					
				EditorGUILayout.BeginHorizontal ();
				init.sdk = (HGarterInit.Sdk)EditorGUILayout.EnumPopup ("SDK", (HGarterInit.Sdk)init.sdk);
				EditorGUILayout.EndHorizontal();

				// ----- switching basic / full-lite -----
				if (init.sdk != HGarterInit.Sdk.Basic) {
					// ----- full & lite sdk -----
						
					// EVENTS
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.PropertyField (serializedObject.FindProperty ("events"), true);
					EditorGUILayout.EndHorizontal ();
				
					// grouping
					if (init.sdk == HGarterInit.Sdk.Full) { // FULL sdk
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.PropertyField (serializedObject.FindProperty ("property"), true);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("Individual Data Save");
						init.individual = EditorGUILayout.TextField (init.individual);
						EditorGUILayout.EndHorizontal ();
						// settings button
						EditorGUILayout.PrefixLabel ("");
						// feature box
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.PropertyField (serializedObject.FindProperty ("featureBox"), true);
						EditorGUILayout.EndHorizontal ();

					} else { // lite SDK
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.PropertyField (serializedObject.FindProperty ("ingameCurrency"), true);
						EditorGUILayout.EndHorizontal ();

						// features
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.PropertyField (serializedObject.FindProperty ("gamearterFeatures"), true);
						EditorGUILayout.EndHorizontal ();

						// feature box
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.PropertyField (serializedObject.FindProperty ("advancedFeatureBox"), true);
						EditorGUILayout.EndHorizontal ();
					}
							
					// Progress module settings
					EditorGUILayout.BeginHorizontal ();
					if (init.progressModule.mode != HGarterInit.ProgressMode.None) {
						EditorGUILayout.PropertyField (serializedObject.FindProperty ("progressModule"), true);
					} else {
						init.progressModule.mode = (HGarterInit.ProgressMode)EditorGUILayout.EnumPopup ("Progress Mode", (HGarterInit.ProgressMode)init.progressModule.mode);
					}
					EditorGUILayout.EndHorizontal();
					
					// external box config
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.PropertyField (serializedObject.FindProperty ("minimumTimescale"), true);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.PropertyField (serializedObject.FindProperty ("autoSaving"), true);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.PropertyField (serializedObject.FindProperty ("analyticsMode"), true);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.PrefixLabel("");
					// ----- Export data -----
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.HelpBox ("\nOnce you are complete with filling the data, or you updated some data, pres PLAY button and then Export the data on server.\n", MessageType.Info);
					if (GUILayout.Button ("\nExport to Server\n(in play mode only)\n")) {

						// Export settings
						bool errorInData = false;

						List<string> stackDataList = new List<string> ();
						char interpreter = GetInterpreter(init.sdk.ToString ());

						// ----- EVENTS ----- //
						string eJSON = Garter.I.ToJson<HGarterInit.Events[]> (init.events);

						int eventsLength = 0;
						int groupsLength = init.events.Length;
						HashSet<string> groupNameDuplicity = new HashSet<string> ();
						for (int i = 0; i < groupsLength; i++) {
							eventsLength += init.events [i].groupEvents.Length;
							groupNameDuplicity.Add (init.events [i].groupName);
						}
						if (groupNameDuplicity.Count != groupsLength) {
							errorInData = true;
							Debug.LogError ("Duplicity in event group name");
						}
						string[] eventsNames = new string[eventsLength];
						decimal[] eInitValue = new decimal[eventsLength];
						string[] eTrend = new string[eventsLength];

						int eventIndex = 0;
						for (int i = 0; i < groupsLength; i++) {
							int eLength = init.events [i].groupEvents.Length;
							for (int j = 0; j < eLength; j++) {
								eventsNames [eventIndex] = init.events [i].groupEvents [j].nameId;
								eInitValue [eventIndex] = (decimal)init.events [i].groupEvents [j].initialValue;
								eTrend [eventIndex] = init.events [i].groupEvents [j].trend.ToString();

								if (eTrend [eventIndex] == "Decreasing" && eInitValue [eventIndex] <= 0) {
									errorInData = true;
									Debug.LogError ("GARTER | event " + eventsNames [eventIndex] + " | Initial value must be higher than 0");
								} else if (eInitValue [eventIndex] < 0) {
									Debug.LogError ("GARTER | event values | event " + eventsNames [eventIndex] + " | Initial value must be higher or equal 0");
								}
								eventIndex++;
							}
						}
								
						//#if UNITY_EDITOR && UNITY_WEBGL
						//Check inserted values
						if (init.projectId == 0) {
							errorInData = true;
							Debug.LogError ("Project Id cannot be 0");
						}


							// if decreasing trend -> initial value cannot be 0


							// extract taken data to a console + check for duplicity
							HashSet<string> gameEventsDuplicity = new HashSet<string> (); //check itemNames for duplicity
							string gameEventsC = "game events (" + eventsLength + "): ";
							for (int i = 0; i < eventsLength; i++) {
								gameEventsDuplicity.Add (eventsNames [i]); 
								gameEventsC += eventsNames [i] + ", ";
							}
							if (eventsLength != gameEventsDuplicity.Count) {
								errorInData = true;
								Debug.LogError ("Event Duplicity");
							}
							//#endif

							// ----- ITEMS ----- //

							string iJSON = Garter.I.ToJson<HGarterInit.Property> (init.property);

							int itemsChildren = init.property.items.Length;
							int accessoryLength = init.property.accessories.Length;
							int itemsTotalL = itemsChildren + accessoryLength;
							string propertyDataC = "Items (" + itemsTotalL + "): ";
							HashSet<string> itemNamesDuplicity = new HashSet<string> (); //check itemNames for duplicity


							//itemNames: utem0, item1 ... (vse v items)
							string[] itemNames = new string[itemsTotalL];

							//itemsStates: -1 = locked, 0 = unlocked, 1 = upgrade 1 ...
							int[] itemsState = new int[itemsTotalL];

							//propertySkillName: ["attack","defense"] - undefined number of skills -> jagged array
							string[][] itemSkillName = new string[itemsTotalL][];

							//propertySkillPerformance: [100,20]
							float[][] itemSkillPerformance = new float[itemsTotalL][];

							//initial dependencies item1 + item 7 = e.g. weapon1 + laser3
							List<string> dependencies = new List<string> ();

							if (interpreter.Equals ('F')) {
								// ------------------------------------------------------------------------------------------------------ //

								// Items
								for (int i = 0; i < itemsChildren; i++) {

									//check and add itemName
									string itemName = init.property.items [i].nameId;
									itemNames [i] = itemName; 

									string iState = init.property.items [i].initialState.ToString ();
									switch (iState) { 
									case "locked":
										itemsState [i] = -2;
										break;
									case "unlocked":
										itemsState [i] = -1;
										break;
									case "bought":
										itemsState [i] = 0;
										break;
									case "selected":
										itemsState [i] = 0;
										stackDataList.Add (itemName); //add to stackDataList
										break;
									}

									//check and add dependency
									string dependency = init.property.items [i].initialAccessories; // nastav defaultne
									if (!string.IsNullOrEmpty(dependency)) {
										dependencies.Add (itemName+"#"+dependency);
									}

									//#if UNITY_EDITOR && UNITY_WEBGL
									itemNamesDuplicity.Add (itemName);
									propertyDataC += "|| " + itemName + ": state=" + itemsState [i];
									propertyDataC += ", dependencies: " + dependency + ", properties: "; //searching according to top item (index 0 after parsing)
									//#endif

									//check and add Property
									int propertiesChild = init.property.items [i].initialProperties.Count;

									List<HGarterInit.InitialProperties> bpData = (List<HGarterInit.InitialProperties>)init.property.items [i].initialProperties;

									itemSkillName [i] = new string[propertiesChild];
									itemSkillPerformance [i] = new float[propertiesChild];
									for (var k = 0; k < propertiesChild; k++) {
										HGarterInit.InitialProperties bp = bpData [k];
										itemSkillName [i] [k] = bp.parameterName;
										itemSkillPerformance [i] [k] = bp.parameterValue;

										#if UNITY_EDITOR && UNITY_WEBGL
										if (itemSkillName [i] [k] == "") {
										Debug.LogError ("GameArter | Property name of *" + itemName + "* is empty. Please, fill it.");
										}
										propertyDataC += itemSkillName [i] [k] + "=" + itemSkillPerformance [i] [k] + " ";
										#endif
									}

								}
								// ------------------------ accessories ----------------------------- //
								for(var i=0; i<accessoryLength;i++){
									// join items and accessories
									int indexY = itemsChildren + i;

									itemNames [indexY] =  init.property.accessories [i].nameId;
									itemNamesDuplicity.Add (itemNames [indexY]);
									// state
									string iState = init.property.accessories [i].initialStateAsc.ToString ();
									switch (iState) { 
									case "locked":
										itemsState [indexY] = -2;
										break;
									case "unlocked":
										itemsState [indexY] = -1;
										break;
									case "bought":
										itemsState [indexY] = 0;
										break;
									}

									// properties
									int propertiesChild = init.property.accessories [i].initialProperties.Count;
									List<HGarterInit.InitialProperties> bpData = init.property.accessories [i].initialProperties;
									itemSkillName [indexY] = new string[propertiesChild];
									itemSkillPerformance [indexY] = new float[propertiesChild];
									for (var k = 0; k < propertiesChild; k++) {
										HGarterInit.InitialProperties bp = bpData [k];
										itemSkillName [indexY] [k] = bp.parameterName;
										itemSkillPerformance [indexY] [k] = bp.parameterValue;

										//-------------------------------#if UNITY_EDITOR && UNITY_WEBGL
										if (itemSkillName [indexY] [k] == "") {
											errorInData = true;
											Debug.LogError ("empty skill parameter");
										}
										//-----------------------------#endif
									}
								}
									
								//#if UNITY_EDITOR && UNITY_WEBGL
								//check item name for duplicity
								if (stackDataList.Count == 0) {
								Debug.LogWarning ("GameArter | No items inside stack. Is this right?");
								}
								if (itemNamesDuplicity.Count != itemNames.Length) {
									errorInData = true;
									Debug.LogError ("Item Name Duplicity");
								} else {
									//Debug.Log (propertyDataC);
								}
								//#endif
							}

							//----------------------------#if UNITY_EDITOR && UNITY_WEBGL

							//check dependencies, whether does not contain undefined item name
							for (int i=0;i<dependencies.Count;i++){
								try{
									string dependencyArr = dependencies[i].Split (new string[] { "#" }, System.StringSplitOptions.None)[1]; // value of key
									string[] itemsArr = dependencyArr.Split (new string[] { "-" }, System.StringSplitOptions.None); // split value

									for (int j=0;j<itemsArr.Length;j++){
										int dItemI = System.Array.IndexOf (itemNames,itemsArr[j]); // does certain item exist in accessories?
										if(dItemI != -1){
											int dItemState = itemsState[dItemI];
											if(dItemState < 0){
												errorInData = true;
												Debug.LogError ("Dependency - item " + itemsArr[j]+ " has state "+dItemState+" (not bought) and therefore is not available for user");
											}
										} else {
											errorInData = true;
											Debug.LogError ("Dependency - undefined accessory name " + itemsArr[j]+". Check Initial Accessoriesboxes of Property Items.");
										}
									}

								} catch {
									Debug.LogWarning("GameArter | Dependency check algorithm failed");
								}
							}
							//------------------------#endif

							string[] stackData = new string[0];
							int stackDataListChilds = stackDataList.Count;
							if (stackDataListChilds > 0) {
								stackData = new string[stackDataListChilds];
								for (int i = 0; i < stackDataListChilds; i++) {
									stackData [i] = stackDataList [i];
								}
							}

							string initDataJson = "";
							if (interpreter.Equals ('F')) { //individual...
								InitExportFull initData = new InitExportFull ();
								initData.m = multiplayer;
								initData.mt = init.minimumTimescale;
								initData.ej = eJSON;
								initData.a = (byte)((init.autoSaving == HGarterInit.EnabledDisabled.Enabled) ? 1 : 0);
								initData.c = 0;
								initData.p = GetProgressMode (init.progressModule.mode);
								//Debug.Log ("progress mode: "+initData.p);
								initData.ij = iJSON;
								initData.its = itemsState;
								initData.s = stackData;
								initData.l = init.individual;
								initDataJson = JsonConvert.SerializeObject (initData);
							} else { //lite
								InitExport initData = new InitExport ();
								initData.m = multiplayer;
								initData.mt = init.minimumTimescale;
								initData.ej = eJSON;
								initData.a = (byte)((init.autoSaving == HGarterInit.EnabledDisabled.Enabled) ? 1 : 0);
								initData.c = (byte)((init.ingameCurrency == HGarterInit.Active.Yes) ? 1 : 0);
								initData.p = GetProgressMode (init.progressModule.mode);
								initDataJson = JsonConvert.SerializeObject (initData);
							}
								
							if (Application.isPlaying) {
								if (!errorInData) {
									Garter.I.ExportData <GarterWWW>(initDataJson, callback => ServerCallback (true, callback));
								} else {
									Debug.LogError ("GAMEARTER | Please, fix gamearter errors before exporting.");
								}
							} else {
								Debug.LogError ("Editor must be in play mode to be possible to Export the data.");
							}
						}
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.PrefixLabel("");
						// clear saved data btn
						GUILayout.Label ("Clear user's data (For testing purposs)");
						EditorGUILayout.BeginHorizontal ();
						if (GUILayout.Button ("\nSelected user - (Guest / logged)\n")) {
							// Clear data on server and in playerprefs
							if (Application.isPlaying) {
								Garter.I.ClearData <GarterWWW>(callback => ServerCallback (false, callback));
							} else {
								Debug.LogError ("Editor must be in play mode to be possible to clear user's data.\n");
							}
						}
						if (GUILayout.Button ("\nBoth users - Guest + logged user\n")) {
							// Clear data on server and in playerprefs
							if (Application.isPlaying) {
								Garter.I.HEditorClearUserdata <GarterWWW>(callback => ServerCallback (false, callback));
							} else {
								Debug.LogError ("Editor must be in play mode to be possible to clear user's data.");
							}
						}
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.PrefixLabel("");
						EditorGUILayout.BeginHorizontal ();
						if (GUILayout.Button ("\nGet guest's all data (for bug diagnostics)\n")) {
							// Clear data on server and in playerprefs
							if (Application.isPlaying) {
								Garter.I.HEditorAllGuestData();
							} else {
								Debug.LogError ("Editor must be in play mode to be possible to clear user's data.");
							}
						}
						EditorGUILayout.EndHorizontal ();
						// Advanced settings
						EditorGUILayout.BeginHorizontal ();
						//EditorGUILayout.HelpBox ("Clearing saved data. (For testing purposs)", MessageType.Info);
						if (GUILayout.Button ("\nAdvanced configuration\nRewards, Badges, Leaderboards, Shop, Progress...\n")) {
							Application.OpenURL ("https://developers.gamearter.com/projects");
						}
						EditorGUILayout.EndHorizontal ();
						
					} else {
						// ----- basic sdk -----
						EditorGUILayout.PrefixLabel ("");
						// feature box
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.PropertyField (serializedObject.FindProperty ("featureBox"), true);
						//init.featureBox.settingsButton = (HGarterInit.BtnVisibility)EditorGUILayout.EnumPopup ("Settings vutton visibility", (HGarterInit.BtnVisibility)init.featureBox.settingsButton);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.PropertyField (serializedObject.FindProperty ("minimumTimescale"), true);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.PropertyField (serializedObject.FindProperty ("analyticsMode"), true);
						EditorGUILayout.EndHorizontal();

						// approve button
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.HelpBox ("\nOnce you are complete with filling the data, pres PLAY button and then Export the data on server.\n", MessageType.Info);
						if (GUILayout.Button ("\nExport on Server\n(in play mode only)\n")) {
							if (Application.isPlaying) {
								InitBasicMode initData = new InitBasicMode ();
								initData.m = multiplayer;
								initData.mt = init.minimumTimescale;
								Garter.I.ExportData <GarterWWW>(JsonConvert.SerializeObject (initData), callback => ServerCallback (true, callback));
							} else {
								Debug.LogError ("Editor must be in play mode to be possible to Export the data.");
							}
						}
						EditorGUILayout.EndHorizontal ();
					}
			}
		}
		//end of active / deactive sdk
		serializedObject.ApplyModifiedProperties();
	}

	private void ServerCallback(bool export, GarterWWW www){
		if (www.error != null) {
			Debug.LogError ("Garter | Server Callback | "+www.error);
		} else {
			Debug.Log ("GameArter | Server Callback | "+ www.text);
			if (export) {
				string requestUrlAddress = Garter.I.GetServerAddress () + "/tmpDataAuthorization?gid=" + projectId + "&pv=" + projectVer;
				Application.OpenURL (requestUrlAddress);
				Debug.LogWarning ("If no page in a browser was opened, open a browser manually and request following url address: "+requestUrlAddress);
				// CELAR USER DATA - SERVER + PLAYERPREFS TO PREVENT from ERRORS
				// stop editor
				UnityEditor.EditorApplication.isPlaying = false;
			} else { // clear data
				Garter.I.EditorMode("getInit"); //(hack for download)
			}
		}
	}

	private char GetInterpreter(string intr){
		char interpreter = 'B';
		if (intr == "Full") {
			interpreter = 'F';
		} else if (intr == "Lite") {
			interpreter = 'L';
		}
		return interpreter;
	}

	private byte GetProgressMode(HGarterInit.ProgressMode modeOption){
		byte progressMode = 0; // None
		switch (modeOption) {
		case HGarterInit.ProgressMode.Sdk: progressMode = 1; break;
		case HGarterInit.ProgressMode.Individual: progressMode = 2; break;
		}
		return progressMode;
	}
		
	[System.Serializable]
	internal class InitExport //post data
	{
		public string ej; // event json
		public bool m; // multiplayer
		public float mt; // minimal timestamp
		public byte a; // auto saving
		public byte c; // game currency
		public byte p; // active sdk progress?
	}
		
	[System.Serializable]
	internal class InitExportFull : InitExport
	{
		public string ij; // item json
		public int[] its; // item state
		public string[] s; // stack data

		public string l; // individual data
	}
	[System.Serializable]
	internal class InitBasicMode
	{
		public bool m; // multiplayer
		public float mt; // minimal timestamp
	}
}
