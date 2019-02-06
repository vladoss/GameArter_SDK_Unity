using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarterAssetsManagement {

	/**
	 * DEFINITION FOR WORK WITH ASSETS
	 * AVAILABLE REQUESTS:
	 * new AssetManagement.POST()
	 * new AssetManagement.GET()
	 * new AssetManagement.UPDATE()
	 * new AssetManagement.DUPLICATE()
	 * new AssetManagement.DELETE()
	 * new AssetManagement.Rate()
	 */

	// --------- POST OPT ------------ //
	[System.Serializable]
	public class PostOpt : DuplicateOpt //post data
	{
		public string name;
		public string img;
		public string metaData; // "mode"
		public string data;
		public Accessability accessability;
		public Cloneable cloneable;
	}
	public void Post(PostOpt opt, Action<GarterWWW> callback){
		SendRequest(Garter.AssetsManagementType.POST, Garter.I.ToJson (opt), callback);
	}

	// --------- GET OPT ------------ //
	[System.Serializable]
	public class GetOpt : DuplicateOpt //post data
	{
		public GetReq dataType;
		public Accessability accessability;
		public string metaData; // mode
		public OrderBy orderBy;
		public byte limit;
	}
	public enum GetReq {
		FILE,
		LIST
	}
	public enum Accessability {
		ALL,
		PUBLIC,
		PRIVATE
	}
	public enum OrderBy {
		RATING,
		RATING_DESC,
		CREATED,
		CREATED_DESC,
		DOWNLOADS,
		DOWNLOADS_DESC
	}
	public enum Cloneable {
		NO,
		YES
	}
		
	[System.Serializable]
	public class AssetsCbFormat : DuplicateOpt {
		public string data;
		//public Texture2D img;
		public Cloneable cloneable;
		public List<AssetListMember> list;
	}
	[System.Serializable]
	public class AssetListMember : DuplicateOpt {
		public string name;
		public byte rating;
		public uint votes;
		public uint downloads;
		public string data;
		public DateTime created;
		public Cloneable cloneable;
		public Accessability accessability;
	}
		
	public void Get(GetOpt opt, Action<GarterWWW> callback){ // FILE / LIST
		SendRequest(Garter.AssetsManagementType.GET, Garter.I.ToJson (opt), callback);
	}

	// --------- UPDATE OPT ------------ //
	[System.Serializable]
	public class UpdateOpt : DuplicateOpt //post data
	{
		public UpdateType updateType;
		public string metaData; // mode
		public string data;
		public string img;
	}
	public enum UpdateType {
		FILE,
		NAME,
		ACCESSIBILITY,
		CLONEABILITY
	}
	public void Update(UpdateOpt opt, Action<GarterWWW> callback){ // map / rename / access - public_private / owner
		SendRequest(Garter.AssetsManagementType.UPDATE, Garter.I.ToJson (opt), callback);
	}

	// --------- DUPLICATE OPT ------------ //
	[System.Serializable]
	public class DuplicateOpt //post data
	{
		public uint assetId;
	}
	public void Duplicate(DuplicateOpt opt, Action<GarterWWW> callback){  // EXTEND OWNER
		SendRequest(Garter.AssetsManagementType.DUPLICATE, Garter.I.ToJson (opt), callback);
	}


	// --------- DELETE OPT ------------ //
	[System.Serializable]
	public class DeleteOpt : DuplicateOpt {}
	public void Delete(DeleteOpt opt, Action<GarterWWW> callback){
		SendRequest(Garter.AssetsManagementType.DELETE, Garter.I.ToJson (opt), callback);
	}

	// --------- RATE OPT ------------ //
	public class RateOpt  : DuplicateOpt //post data
	{
		public byte value; 
	}
	public void Rate(RateOpt opt, Action<GarterWWW> callback){ // like = 100, dislike = 0
		SendRequest(Garter.AssetsManagementType.RATE, Garter.I.ToJson (opt), callback);
	}

	private void SendRequest(Garter.AssetsManagementType type, string data, Action<GarterWWW> callback){
		Garter.I.AssetsManagement <GarterWWW>(type, data, callback);
	}
}