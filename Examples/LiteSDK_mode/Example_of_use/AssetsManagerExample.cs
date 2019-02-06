using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class AssetsManagerExample : MonoBehaviour {

	// Example of assets management feature - https://developers.gamearter.com/docs/unity/assets-management.php

	private void CallbackFunctionForAssets(GarterWWW www){
		if (string.IsNullOrEmpty (www.error)) {
			GarterAssetsManagement.AssetsCbFormat responseData = Garter.I.FromJson<GarterAssetsManagement.AssetsCbFormat> (www.text);
			List<GarterAssetsManagement.AssetListMember> listOfAssets = responseData.list;
			uint assetId = responseData.assetId;
			//Texture2D assetImage = responseData.img;
			GarterAssetsManagement.Cloneable clonePermission = responseData.cloneable;
			string assetData = responseData.data;

			Debug.Log ("downloadedAssetId: " + assetId);
			Debug.Log ("asset - cloneability: " + clonePermission);
			Debug.Log ("asset - data: " + assetData);
			if (listOfAssets != null) {
				Debug.Log ("--- List of assets ---");
				for (int i = 0; i < listOfAssets.Count; i++) {
					Debug.Log ("assetId: " + listOfAssets [i].assetId);
					Debug.Log ("asset name: " + listOfAssets [i].name);
					Debug.Log ("asset votes: " + listOfAssets [i].votes);
					Debug.Log ("asset rating: " + listOfAssets [i].rating);
					Debug.Log ("asset downloads: " + listOfAssets [i].downloads);
					Debug.Log ("asset cloneable: " + listOfAssets [i].cloneable);
					Debug.Log ("asset created: " + listOfAssets [i].created);
					Debug.Log ("asset accessability: " + listOfAssets [i].accessability);
				}
			}
		} else {
			Debug.LogError (www.error);
		}
	}
		
	/**
	 * Feature for users asset management.
	 * users asset is for example a map created by user
	 * users can create, update, rename, duplicate, rate and delete assets.
	*/
	// Asset management example
	GarterAssetsManagement AM = new GarterAssetsManagement();
	// 1) POST created asset to server
	public void CreateAndPostNewAsset(){
		GarterAssetsManagement.PostOpt po = new GarterAssetsManagement.PostOpt ();
		po.assetId = 0; // is extension of asset (0= totally new asset) 
		po.name = "My map"; // set map name
		po.img = null; // set map img
		po.metaData = "zombie"; // for option of additional filtration
		string longData = "0ТТТ21Д101.908Д-0.5Д78.34435Д0.7Д0.7Д0.7Д0Д0Д0ДД5КТ0Т1Т";
		po.data = longData; // data
		po.accessability = GarterAssetsManagement.Accessability.PUBLIC;
		AM.Post (po, CallbackFunctionForAssets);
		// returns state - success / error
	}
		
	// 2) GET LIST OF ASSETS
	public void GetListOfAssets(){
		GarterAssetsManagement.GetOpt po = new GarterAssetsManagement.GetOpt ();
		po.dataType = GarterAssetsManagement.GetReq.LIST; // list of maps
		po.accessability = GarterAssetsManagement.Accessability.ALL; // all assets a user can use
		po.metaData = "001"; // load only assets with metadata zombie
		po.orderBy = GarterAssetsManagement.OrderBy.RATING_DESC; // list of best
		po.limit = 30; // load list with maximum number of 30 results
		AM.Get (po, CallbackFunctionForAssets);
		// returns list of assets. Every asset has attached assetId, name, image, rating, votes, downloads
	}

	// 3) GET ASSET TO LOAD
	public void GeAssetToLoad(){
		GarterAssetsManagement.GetOpt po = new GarterAssetsManagement.GetOpt ();
		po.dataType = GarterAssetsManagement.GetReq.FILE; // get 1 asset
		po.assetId = 13; // dwnload data of assetId 1
		AM.Get (po, CallbackFunctionForAssets);
		// returns data and image of selected assetId
	}

	// 4) Update 
	public void UpdateAsset(){
		GarterAssetsManagement.UpdateOpt po = new GarterAssetsManagement.UpdateOpt ();
		po.assetId = 1;
		po.updateType = GarterAssetsManagement.UpdateType.NAME; // what is need to update.
		po.data = "New Asset name";
		AM.Update (po, CallbackFunctionForAssets);
	}

	// 5) duplicate
	public void DuplicateAsset(){
		GarterAssetsManagement.DuplicateOpt po = new GarterAssetsManagement.DuplicateOpt ();
		po.assetId = 1;
		AM.Duplicate (po, CallbackFunctionForAssets);
	}

	// 6) delete asset
	public void DeleteAsset(){
		GarterAssetsManagement.DeleteOpt po = new GarterAssetsManagement.DeleteOpt ();
		po.assetId = 1;
		AM.Delete (po, CallbackFunctionForAssets);
	}

	// 7) Rate
	public void RateAsset(){
		GarterAssetsManagement.RateOpt po = new GarterAssetsManagement.RateOpt ();
		po.assetId = 1;
		po.value = 100; // = 100% = like
		AM.Rate (po, CallbackFunctionForAssets);
	}
}