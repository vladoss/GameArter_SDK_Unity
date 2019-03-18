using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarterWWW {
	public string text;
	public string error;
	//public Texture2D texture;
	public GarterWWW(string data, string error/*, Texture2D texture*/){
		this.text = data;
		this.error = error;
		//this.texture = texture;
	}
}
