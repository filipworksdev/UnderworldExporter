﻿using UnityEngine;
using System.Collections;

public class TradeSlot : GuiBase {

	public bool PlayerSlot=false;
	public int SlotNo;
	//static UWCharacter playerUW;
	//static PlayerInventory pInv;
	public bool pressedDown=false;
	public string objectInSlot;
	public bool Hovering=false;
	public UITexture SlotImage;
	public bool Selected=false;
	private Texture2D Blank;
	public GameObject Indicator;
	// Use this for initialization
	void Start () {
		//playerUW=GameObject.Find ("Gronk").GetComponent<UWCharacter>();
		//pInv=GameObject.Find ("Gronk").GetComponent<PlayerInventory>();
		SlotImage=this.GetComponent<UITexture>();
		Blank = Resources.Load <Texture2D> ("Sprites/Texture_Blank");

		SlotImage.mainTexture=Blank;
	}
	
	// Update is called once per frame
	void Update () {
			Indicator.SetActive(isSelected ());
	}

	void PlayerSlotRightClick()
	{//Toggle selected state
		Selected = !Selected;
	}


	void PlayerSlotLeftClick()
	{
		if (playerUW.playerInventory.ObjectInHand != "")
		{
			//put the object in hand in this slot.
			if (objectInSlot=="")
			{//Empty slot
				objectInSlot=playerUW.playerInventory.ObjectInHand;
				playerUW.playerInventory.ObjectInHand="";
				SlotImage.mainTexture=playerUW.CursorIcon;
				playerUW.CursorIcon=playerUW.CursorIconDefault;
			}
			else
			{//Swap the objects
				string tmp;
				tmp = objectInSlot;
				objectInSlot=playerUW.playerInventory.ObjectInHand;
				playerUW.playerInventory.ObjectInHand=tmp;
				playerUW.CursorIcon= playerUW.playerInventory.GetGameObject(tmp).GetComponent<ObjectInteraction>().GetInventoryDisplay().texture;
				
			}
			
		}
		else
		{
			if (objectInSlot!="")
			{
				//Pickup the object in the slot
				playerUW.playerInventory.SetObjectInHand(objectInSlot);
				playerUW.CursorIcon= playerUW.playerInventory.GetGameObject(objectInSlot).GetComponent<ObjectInteraction>().GetInventoryDisplay().texture;
				objectInSlot="";
				SlotImage.mainTexture=Blank;
			}
			
		}

	}


	void NPCSlotClick()
	{
		Selected=!Selected;
	}


	void OnClick()
	{
		//Left click pickup
		//right click toggle.
		//On hover identify?
		if (PlayerSlot==true)
		{
			if (UICamera.currentTouchID == -2)//right click
			{
				PlayerSlotRightClick ();
			}
			else
			{
				PlayerSlotLeftClick ();
			}
		}
		else
		{
			NPCSlotClick ();
		}

	}

	public bool isSelected()
	{//is it a selected slot with an item in it.
		return ((Selected==true) && (objectInSlot!=""));
	}

	public void clear()
	{
		objectInSlot="";
		SlotImage.mainTexture=Blank;
	}


	public int GetObjectID()
	{
		if (isSelected())
		{
			GameObject obj = GameObject.Find (objectInSlot);
			if (obj!=null)
			{
				ObjectInteraction objInt = obj.GetComponent<ObjectInteraction>();
				if (objInt!=null)
				{
					return objInt.item_id;
				}
			}
		}
		return 0;
	}
	//void OnDoubleClick () 
	//{
	//	Debug.Log ("Doubleclick");
	//}
}
