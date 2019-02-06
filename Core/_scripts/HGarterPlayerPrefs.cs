using UnityEngine;

/// <summary>
/// PlayerPrefs is determined for saving basic game data for guest.
/// </summary>

[HideInInspector]
public class HGarterPlayerPrefs {

	public string[] GetGameData(string projectId, string projectVer){
		return new string[] {
			PlayerPrefs.GetString ("g_" + projectId + "_" + projectVer + "_d"),
			PlayerPrefs.GetString ("g_" + projectId + "_" + projectVer + "_h")
		};
	}

	public void SetGameData(string projectId, string projectVer, string data, string hash, string[] events){
		PlayerPrefs.SetString ("g_" + projectId + "_" + projectVer + "_d", data);
		PlayerPrefs.SetString ("g_" + projectId + "_" + projectVer + "_h", hash);
		string ppEventsId = string.Join (",", events);
		PlayerPrefs.SetString ("g_" + projectId + "_" + projectVer + "_e", ppEventsId); // cmpatibility check
	}

	public string[] GetSaveEvents(string projectId, string projectVer){ // compatibility check
		string oldEvntIdns = PlayerPrefs.GetString ("g_" + projectId + "_" + projectVer + "_e");
		return (!string.IsNullOrEmpty (oldEvntIdns)) ? oldEvntIdns.Split (',') : null;
	}

	public void Clear(string projectId, string projectVer){
		PlayerPrefs.DeleteKey ("g_" + projectId + "_" + projectVer + "_d");
		PlayerPrefs.DeleteKey ("g_" + projectId + "_" + projectVer + "_h");
		PlayerPrefs.DeleteKey ("g_" + projectId + "_" + projectVer + "_e");
		PlayerPrefs.Save ();
	}
}