using UnityEngine;

/// <summary>
/// PlayerPrefs is determined for saving basic game data for guest.
/// </summary>

[HideInInspector]
public class HGarterPlayerPrefs {

    public string[] GetGameData(string projectId, string projectVer) {
        return new string[] {
            PlayerPrefs.GetString ("g_" + projectId + "_" + projectVer + "_d"),
            PlayerPrefs.GetString ("g_" + projectId + "_" + projectVer + "_h")
        };
    }

    public void SetGameData(string projectId, string projectVer, string data, string hash, string[] events) {
        PlayerPrefs.SetString("g_" + projectId + "_" + projectVer + "_d", data);
        PlayerPrefs.SetString("g_" + projectId + "_" + projectVer + "_h", hash);
        string ppEventsId = string.Join(",", events);
        PlayerPrefs.SetString("g_" + projectId + "_" + projectVer + "_e", ppEventsId); // cmpatibility check
    }

    public string[] GetSaveEvents(string projectId, string projectVer) { // compatibility check
        string oldEvntIdns = PlayerPrefs.GetString("g_" + projectId + "_" + projectVer + "_e");
        return (!string.IsNullOrEmpty(oldEvntIdns)) ? oldEvntIdns.Split(',') : null;
    }

    public void Clear(string projectId, string projectVer) {
        PlayerPrefs.DeleteKey("g_" + projectId + "_" + projectVer + "_d");
        PlayerPrefs.DeleteKey("g_" + projectId + "_" + projectVer + "_h");
        PlayerPrefs.DeleteKey("g_" + projectId + "_" + projectVer + "_e");
#if UNITY_ANDROID
        PlayerPrefs.DeleteKey("gamearter_ti");
        PlayerPrefs.DeleteKey("gamearter_em");
        PlayerPrefs.DeleteKey("gamearter_ih_" + projectId);
#endif
        PlayerPrefs.Save();
    }

    public void SetUserAuthData(string tokenId, string email/*, uint projectId = 0, string iHash = null*/)
    {
        if (!string.IsNullOrEmpty(tokenId)) PlayerPrefs.SetString("gamearter_ti", tokenId);
        if (!string.IsNullOrEmpty(email)) PlayerPrefs.SetString("gamearter_em", email);
        /*if (!string.IsNullOrEmpty(iHash) && projectId != 0)
        {
            PlayerPrefs.SetString("gamearter_ih_"+ projectId, iHash);
        }*/
        PlayerPrefs.Save();
    }

    public string[] GetUserAuthData(/*uint projectId*/)
    {
        return new string[] {
            PlayerPrefs.GetString ("gamearter_tid"),
            PlayerPrefs.GetString ("gamearter_em")
        };
    }
}