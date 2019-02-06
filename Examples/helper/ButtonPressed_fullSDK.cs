using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class ButtonPressed_fullSDK : MonoBehaviour{

	/********************* item properties ***************************/
	public void GetItemState(string itemId){
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.ItemAvailability("+itemId+");",Garter.I.ItemAvailability(itemId).ToString(),"Item availability");
	}

	public void GetItemPropertyValue(string itemId_PropertyName){
		string[] attributes = itemId_PropertyName.Split ('_');
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.ItemPropertyValue ("+attributes[0]+", "+attributes[1]+")", Garter.I.ItemPropertyValue (attributes[0], attributes[1]).ToString(), "Performance of item property");
	}
		
	public void GetAccessory(string referentItem){
		string value = Garter.I.GetAccessory (referentItem);
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetAccessory ("+referentItem+")", value, "Following accessories are attached to "+referentItem+" item");
	}

	public void SetAccessory(string reqQuery){
		string[] attributes = reqQuery.Split ('_');
		Garter.I.SetAccessory (attributes[0], attributes[1]);
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.SetAccessory ("+attributes[0]+", "+attributes[0]+")", "-", "Accessories for item "+attributes[0]+" were set)");
	}

	public void GetItemSlot(string itemStringId){
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetSlotOfItem("+itemStringId+")", Garter.I.GetSlotOfItem(itemStringId).ToString(), "Index of item "+itemStringId+" in the slot");
	}

	public void GetSlot(int index){
		string  value = Garter.I.GetSlot (index);
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetSlot ("+index+")", (value != null) ? value.ToString() : null);
	}

	public void SetSlot(string query){
		string[] attributes = query.Split ('_');
		int toSlotIndex = Int32.Parse(attributes [1]);
		string itemStringId = null, text = "Slot "+toSlotIndex+" was cleared";
		if (!String.IsNullOrEmpty (attributes [0])) {
			itemStringId = attributes [0];
			text = "Item "+itemStringId+" was attached to slot "+toSlotIndex+".";
		}

		string resultReturned = Garter.I.SetSlot (toSlotIndex, itemStringId);
		if(string.IsNullOrEmpty(resultReturned)) text = "Slot "+toSlotIndex+" is not available. Create it first by add slot button";
		
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.SetSlot ("+toSlotIndex+", "+itemStringId+");", resultReturned, text);
	}

	public void ClearSlot(int slotIndex){
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.SetSlot ("+slotIndex+");", Garter.I.SetSlot(slotIndex).ToString(), "Slot index "+slotIndex+" was cleared");
	}

	/***************************** slots (other functions) ***************************/

	public void AddSlot(){
		int value = Garter.I.AddSlot ();
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.AddSlot ()", value.ToString(), "Extend slots about one");
	}

	public void GetAllSlots(){
		string[] value5 = Garter.I.GetSlots ();
		string slots = "";
		int i = 0;
		foreach (string key in value5) {
			slots += value5 [i]+", ";
			i++;
		}
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetSlots ()", slots, "Content of all slots");
	}
		
	/***************************** server ***************************/
	public void PostManually(){
		Garter.I.PostDataManually();
		StaticHelpersGarterSDK.SdkDebugger("Garter.I.PostDataManually()","-","Data posted to server manually.");
	}
}