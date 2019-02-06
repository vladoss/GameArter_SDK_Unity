using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[HideInInspector]
public class HGarterBranding : MonoBehaviour {

	public Brand brand;
	public enum Brand {
		/*Brand_Logo, Brand_Emblem, GameArter_Logo,*/ individual
	}
	public Texture logoTexture;
	public string logoRedirect;
	private GameObject activeObject;

	// Use this for initialization
	void Awake () {
		activeObject = this.gameObject;
		if (brand == Brand.individual) { // individual buttons - texture is inside the game
			LoadIndividualLogo ();
		} /*else { // wait for dynamic logo data - then download logo and load it - object names - Brand_Logo, Brand_Emblem, GameArter_Logo
			logoRedirect = null;
			byte brandIndex = 0; //"gamearter"
			switch(brand){
			case Brand.Brand_Logo: brandIndex = 1; break; //"brand_l"
			case Brand.Brand_Emblem: brandIndex = 2; break; //"brand_e"
			}
			BrandingLogoMng (brandIndex);
		}*/
	}
	/*
	private void BrandingLogoMng(byte brandIndex){
		object[] brandObject = Garter.I.BrandModule (brandIndex);
		// 1) check info availability
		if (string.IsNullOrEmpty((string)brandObject [0])) { // unknown logo name - try it later
			//1) hide logos - remove img element

		//2) try it later
			//if(remainingAttepmts > 0){ // risk of loop - check na game load (game initialized/loaded) - login response
				
				// CALLBACK FUNKCE - NASLEDUJE BRANDING LOGO REKAPITULACE
			//}
			//Garter.I.WaitingForBranding(res => BrandingLogoMng(res));
			// PRES CAS
				
		} else { // known logo name
			if ((Texture2D)brandObject[1] == null) { // texture is null (first load)
				string url = "https://data.pacogames.com/gplayer/branding/" + (string)brandObject[0] + ".png";
				Garter.I.DownloadDataFile (url, res => ImgDownloaded (url, brandIndex, res)); // Download and display
			} else {
				DisplayImg ((Texture2D)brandObject[1]);
			}

		}
	}

	private void ImgDownloaded(string url, byte brandIndex, WWW www){
		if (www.error == null) {
			Texture2D downloadedTxt = www.texture;
			Texture2D texture = new Texture2D (downloadedTxt.width, downloadedTxt.height, TextureFormat.DXT5, false);
			texture = downloadedTxt;
			DisplayImg (texture);
			Garter.I.BrandModule(brandIndex, texture); // save texture
		} else {
			//Debug.Log (brand.ToString());
			GameObject.Find ("GameArter_Branding/"+brand.ToString()).SetActive (false);
		}
	}
	private void DisplayImg(Texture2D texture){
		//GameObject gameObject = GameObject.Find ("GameArter_Branding/"+brand.ToString()); // activeObject??
		//RawImage img = gameObject.GetComponent<RawImage> ();
		RawImage img = activeObject.GetComponent<RawImage> ();
		img.texture = texture;
		if (Garter.I.ClickableLink(brand.ToString())) {
			ClickableButton (gameObject);
		}
	}
	*/

	private void LoadIndividualLogo(){
		if (logoTexture != null) {
			// display logo
			activeObject.AddComponent<RawImage> ().texture = logoTexture;
			if (Garter.I.ClickableLink(brand.ToString())) {
				ClickableButton (gameObject);
			}
		} else {
			Debug.LogWarning ("Texture for "+this.gameObject+" is not defined");
		}
	}

	private void ClickableButton(GameObject gameObject){
		// clickable - functionality
		EventTrigger et = gameObject.AddComponent<EventTrigger> ();
		EventTrigger.Entry entry = new EventTrigger.Entry ();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
		et.triggers.Add(entry);

		// clickable - visibility
		Button btn = gameObject.AddComponent<Button>();
		btn.transition = Selectable.Transition.ColorTint;
		ColorBlock btnc = btn.colors;
		btnc.highlightedColor = new Color32 (255, 255, 255, 175);
		btn.colors = btnc;
	}

	private void OnPointerDownDelegate(PointerEventData data)
	{
		Garter.I.OpenWebPage (logoRedirect, brand.ToString().ToLower()); // definuj url
	}
}