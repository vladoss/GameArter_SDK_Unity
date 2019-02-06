using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text; //encoding
using UnityEngine.UI;



public class HGarterScreenShot : MonoBehaviour {
	/*
	HGarterGui gui = new HGarterGui ();

	public void TakeScreenShot(){
		Camera camera = GetComponent<Camera> ();
		int width = Screen.width;
		int height = Screen.height;

		RenderTexture rt = new RenderTexture(width, height, 24);
		camera.targetTexture = rt;

		Texture2D snapShot = new Texture2D( width, height, TextureFormat.RGB24, false );
		camera.Render();
		RenderTexture.active = rt;
		snapShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		camera.targetTexture = null;
		RenderTexture.active = null;
		Destroy(rt);
		byte[] bytes = snapShot.EncodeToJPG();
		//System.IO.File.WriteAllBytes("testpicture.jpg", bytes);//saves to the asset folder
		//Debug.Log (Camera.current);
		snapShot.LoadImage (bytes);

		//gui.ScreenShot (snapShot);


		CallServer (bytes, result => SetUserPhoto(result));

	}

	private void SetUserPhoto(WWW www){
		//Debug.Log (www.error);
		Debug.Log (www.text);
	}

	private void CallServer (byte[] data, Action<WWW> callback)  //1 - get, 2 - set, 0 - get img
	{
		string url = "http://localhost:81/img";
		ServerConnection (data, url, callback);
	}



	//private Dictionary<string, string> headers = null;
	private void ServerConnection (byte[] data, string url, Action<WWW> callback)
	{
		WWWForm form = new WWWForm ();


		//form.AddField("action", "level upload");
		//form.AddField ("file", "file");
		form.AddBinaryData ("file", data, "testimg.jpeg", "image/jpg");
		//form.AddField("frameCount", Time.frameCount.ToString());
		//form.AddBinaryData ("file", data, "printscreen.jpg", "image/jpg");
		WWW www = new WWW (url, form);

		StartCoroutine (ServerCallback (www, callback));
	}
	private IEnumerator ServerCallback(WWW www, Action<WWW> callback)
	{
		yield return www;
		callback (www);
	}
	*/
}
