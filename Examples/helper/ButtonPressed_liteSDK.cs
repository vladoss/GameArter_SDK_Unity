using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public class ButtonPressed_liteSDK : MonoBehaviour {
	private DataManagerExample garterLite = null;

	void Awake(){
		try{
			garterLite = GameObject.Find ("ExampleObjectForSdkInteraction").GetComponent<DataManagerExample> ();
		} catch {
		}
	}

	public void PostDataViaMiddleman(){
		StaticHelpersGarterSDK.SdkDebugger("garterLite.PostData (false);","-","Check PostData () in GarterLite script for more info.");
		garterLite.PostDataInMeantime ();
	}

	public void PostDataDirectly(){
		StaticHelpersGarterSDK.SdkDebugger("garterLite.PostData ();","-","Check PostData () in GarterLite script for more info.");
		garterLite.PostData ();
	}

	public void GetData(){
		StaticHelpersGarterSDK.SdkDebugger("garterLite.GetData ()",(Garter.I.GetData<string>("my-key")),"Check GetData () in GarterLite script for more info.");
		garterLite.GetData ();
	}
		
	public void UpdateCurrency(){
		System.Random r = new System.Random ();
		decimal balanceChange = (decimal)r.Next (-10, 10);
		decimal c = Garter.I.LocalCurrency(balanceChange);
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.LocalCurrency("+balanceChange+")",c.ToString("0.000"),"-");

		GameObject userFunds = GameObject.Find ("UserFunds");
		Text userFundsTxt = userFunds.GetComponent<Text> ();
		userFundsTxt.text = "User funds: " + c;
	}

	public void GetCurrencyFunds(){
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.LocalCurrency()",Garter.I.LocalCurrency().ToString("0.000"),"-");
	}
}