﻿using UnityEngine;
using System.Collections;

public class a_do_trap_emeraldpuzzle : trap_base {

	public bool hasExecuted;

	public override void ExecuteTrap (int triggerX, int triggerY, int State)
	{
		if (hasExecuted==false)
		{
			//Check if 4 emeralds on on top of the plinths.
			if (CheckPlinths())
			{
				CreateRuneStone(253);				
				hasExecuted=true;
			}
		}
	}


	private void CreateRuneStone(int ItemID)
	{
		string Item= ItemID.ToString("000");
		GameObject myObj=  new GameObject("SummonedObject_" + playerUW.PlayerMagic.SummonCount++);
		myObj.layer=LayerMask.NameToLayer("UWObjects");
		myObj.transform.parent=GameWorldController.instance.LevelMarker();
		//myObj.transform.position = playerUW.playerInventory.InventoryMarker.transform.position;
		//myObj.transform.parent=playerUW.playerInventory.InventoryMarker.transform;
		ObjectInteraction.CreateObjectGraphics(myObj,_RES +"/Sprites/Objects/Objects_224",true);
		ObjectInteraction.CreateObjectInteraction(myObj,0.5f,0.5f,0.5f,0.5f, _RES +"/Sprites/Objects/Objects_224", _RES +"/Sprites/Objects/Objects_"+Item, _RES +"/Sprites/Objects/Objects_" +Item, ObjectInteraction.RUNE, 224, 1, 40, 0, 1, 1, 0, 1, 1, 0, 0, 1);
		
		myObj.AddComponent<RuneStone>();

		myObj.transform.position = new Vector3(64.5f,4.0f,24.5f);
		WindowDetect.UnFreezeMovement(myObj);
	}


	private bool CheckPlinths()
	{
		if (CheckArea(new Vector3(59.4f,3.9f,27.0f),1.2f,167) ==true)
		{
			if (CheckArea(new Vector3(59.4f,3.9f,17.5f),1.2f,167) ==true)
			{
				if (CheckArea(new Vector3(69.0f,3.9f,17.5f),1.2f,167) ==true)
				{
					if (CheckArea(new Vector3(69.0f,3.9f,27.0f),1.2f,167) ==true)
					{
						return true;	
					}	
				}	
			}
		}
		return false;
	}


	private bool CheckArea(Vector3 centre, float radius, int item_id) 
	{
		Collider[] hitColliders = Physics.OverlapSphere(centre, radius);
		int i = 0;
		while (i < hitColliders.Length) {
			if (hitColliders[i].gameObject.GetComponent<ObjectInteraction>()!=null)
			{
			if (hitColliders[i].gameObject.GetComponent<ObjectInteraction>().item_id==item_id)
			    {
					Debug.Log("found " + hitColliders[i].gameObject.name);
				return true;
				}
			}
			i++;
		}
	return false;
	}

	public override void PostActivate ()
	{
	//Stop trap from destroying itself.

	}
}
