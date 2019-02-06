using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HideInInspector]
public class HGarterClickListener : MonoBehaviour {

	public void LeaderBoardBtn(){
		Garter.I.OpenSdkWindow ("leaderboard");
	}
	public void BadgeBtn(){
		Garter.I.OpenSdkWindow ("badge");
	}
	public void ShopBtn(){
		Garter.I.OpenSdkWindow ("shop");
	}
	public void LoginBtn(){
		Garter.I.OpenSdkWindow ("login");
	}
	public void SettingsBtn(){
		Garter.I.OpenSdkWindow ("settings");
	}
	public void ShareBtn(){
		Garter.I.OpenSdkWindow ("share");
	}
	public void UserProfile(){
		Garter.I.OpenSdkWindow ("profile");
	}
	public void OpenGift(){
		Garter.I.OpenSdkWindow ("gift");
	}
}
