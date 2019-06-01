using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using UnityEngine.UI;

public class StaticHelpersGarterSDK : MonoBehaviour {
	/***************************** debugger ***************************/
	public static void SdkDebugger(string request, string response, string note = "-"){
		GameObject[] sdkDebugger = new GameObject[3]{GameObject.Find("Debugger/Garter_Req/Request"), GameObject.Find("Debugger/Garter_Res/Response"), GameObject.Find("Debugger/Garter_Note/Note")};
		sdkDebugger [0].GetComponent<Text> ().text = request;
		sdkDebugger [1].GetComponent<Text> ().text = response;
		sdkDebugger [2].GetComponent<Text> ().text = note;
	}

    /***************************** General Features ***************************/

    public void SendToAnalytics(){
		Garter.I.AnalyticsEvent ("item-used","whatItem","fromWhere");
		SdkDebugger("Garter.I.ToAnalytics (\"item-used\", \"item1\", \"stack\")","-","User used item1 from his stack");
	}

	public void NextScene(){
		SceneManager.LoadScene (1);
	}

	public void LoadScene(int sceneIndex){
		SceneManager.LoadScene (sceneIndex);
	}
}
