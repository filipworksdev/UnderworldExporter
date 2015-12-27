﻿using UnityEngine;
using System.Collections;
using RAIN.BehaviorTrees;
using RAIN.Core;
using RAIN.Minds;

public class NPC : object_base {

	//TODO: these need to match the UW npc_goals
	//The behaviour trees need to be updated too.
	public const int AI_STATE_IDLERANDOM = 0;
	public const int AI_STATE_COMBAT = 1;
	public const int AI_STATE_DYING = 2;
	public const int AI_STATE_STANDING = 3;
	public const int AI_STATE_WALKING = 4 ;

	public string NavMeshRegion;

	public int npc_whoami;

	public int npc_xhome;        //  x coord of home tile
	public int npc_yhome;        //  y coord of home tile
//	public int npc_whoami;       //  npc conversation slot number
	public int npc_hunger;
	public int npc_health;
	public int npc_hp;
	public int npc_arms;          // (not used in uw1)
	public int npc_power;
	public int npc_goal;          // goal that NPC has; 5:kill player 6:? 9:?
	public int npc_attitude;       //attitude; 0:hostile, 1:upset, 2:mellow, 3:friendly
	public int npc_gtarg;         //goal target; 1:player
	public int npc_talkedto;      // is 1 when player already talked to npc
	public int npc_level;
	public int npc_name;       //    (not used in uw1)

	//public int npc_deathvariable;	//Quest variable to set when the character is killed
	public bool NPC_DEAD;

	//Added by myself
	public bool Poisoned;

	//TODO: The state should be replaces with a combination of the above variables.
	public int state=0; //Set state when not in combat or dying.


	//For applying spell effects to NPCs
	public SpellEffect[] NPCStatusEffects=new SpellEffect[3];

	//public static UWCharacter playerUW;
	private static bool playerUWReady;
	private GoblinAI Gob;
	private AIRig ai;

	// Use this for initialization
	void Start () {
		base.Start();

		Gob = this.GetComponent<GoblinAI>();
		ai = this.GetComponentInChildren<AIRig>();

		if (playerUW==null)
		{
			playerUW = GameObject.Find ("Gronk").GetComponent<UWCharacter>();
		}
		//playerUW = GameObject.Find ("Gronk").GetComponent<UWCharacter>();
		ai.AI.WorkingMemory.SetItem<GameObject>("playerUW",playerUW.gameObject);
		ai.AI.Body=this.gameObject;
	}

	void OnDeath()
	{
		Conversation cnv = this.GetComponent<Conversation>();
		if (cnv!=null)
		{
			if(cnv.OnDeath()==true)
			{
				return;
			}			
		}

		NPC_DEAD=true;
		//Dump items on the floor.
		//
		Container cnt = this.GetComponent<Container>();
		if (cnt!=null)
		{
			Debug.Log ("Spilling " + this.name);
			cnt.SpillContents();//Spill contents is still not 100% reliable so don't expect to get all the items you want.
		}
		else
		{
			Debug.Log ("no container to spill");
		}
	}

	// Update is called once per frame
	void Update () {
		//Gob.isHostile=((npc_attitude==0) && (npc_hp>0));
		//Gob.HP=npc_hp;
		if (NPC_DEAD==true)
		{
			ai.AI.WorkingMemory.SetItem<int>("state",AI_STATE_DYING);//Set to death state.
			return;
		}
		if ( playerUWReady==false)
		{
			if(playerUW!=null)
			{
				ai.AI.WorkingMemory.SetItem<GameObject>("playerUW",playerUW.gameObject);
				playerUWReady=true;
			}
		}
		else
		{
			ai.AI.IsActive= Vector3.Distance(this.transform.position, playerUW.gameObject.transform.position)<=8;
			if (ai.AI.IsActive==false)
			{
				return;
			}
			//if (state== NPC.AI_STATE_WALKING)
			//{//Sets the AI to always be turned towards their next waypoint
			if (ai.AI.Navigator.CurrentPath!=null)
			{
				Vector3 NextPosition = ai.AI.Navigator.CurrentPath.GetWaypointPosition(ai.AI.Navigator.NextWaypoint);
				NextPosition.y= this.transform.position.y;
				//ai.AI.WorkingMemory.SetItem<Vector3>("RotateTowards",ai.AI.Navigator.CurrentPath.GetWaypointPosition(ai.AI.Navigator.NextWaypoint));
				ai.AI.WorkingMemory.SetItem<Vector3>("RotateTowards",NextPosition);
			}
			//}


			if ((npc_hp<=0))
			{
				//if ((npc_deathvariable>0) && (npc_deathvariable<32))
			//	{
				//	playerUW.quest().QuestVariables[npc_deathvariable]=1;
			//	}

				OnDeath();
			//	Debug.Log("NPC Dead");
			}
			else
			{
				if (npc_attitude==0)
					{
					ai.AI.WorkingMemory.SetItem<int>("state",AI_STATE_COMBAT);//Set to combat state.
					}
				else
				{
					ai.AI.WorkingMemory.SetItem<int>("state",state);//Set to idle
				}
			}
		}
	}



	public override bool ApplyAttack(int damage)
	{
		//npc_attitude=0;
		npc_attitude=0;
		npc_hp=npc_hp-damage;

		return true;
	}

	public override bool TalkTo()
	{
		//if (npc_attitude==0)
		//{//Hostile
		//	ml.Add (playerUW.StringControl.GetString (7,1));
		//	return false;
		//}
		//Debug.Log("Talking to " + WhoAmI) ;


		//TODO:Make sure you add the conversation object to the npc!
		if ((npc_whoami == 255))
		{
			//006~007~001~You get no response.
			ml.Add (playerUW.StringControl.GetString (7,1));
		}
		else
		{
			if (npc_whoami==0)
			{
				ObjectInteraction objInt=this.GetComponent<ObjectInteraction>();
				npc_whoami=256+(objInt.item_id -64);
				//Debug.Log ("npc who am i is now " + npc_whoami);
			}
			Conversation x = (Conversation)this.GetComponent("Conversation_"+npc_whoami);
			if (x!=null)
			{	
				UWCharacter.InteractionMode=UWCharacter.InteractionModeInConversation;
				Conversation.CurrentConversation=npc_whoami;
				Conversation.InConversation=true;
				x.WhoAmI=npc_whoami;

				Conversation.maincam=Camera.main;
				UWCharacter.InteractionMode=UWCharacter.InteractionModeTalk;
				InteractionModeControl.UpdateNow=true;				
				//Camera.main.enabled = false;
				StartCoroutine(x.main ());

			}
			else
			{
				ml.Add (playerUW.StringControl.GetString (7,1));
				//chains.ActiveControl=0;//Enable UI Elements
				//chains.Refresh();
			}
		}
		return true;
	}

	public override bool LookAt ()
	{
		string output = playerUW.StringControl.GetFormattedObjectNameUW(objInt,NPCMoodDesc());
		if ((npc_whoami >=1) && (npc_whoami<255)) 
		{
			if (npc_whoami==207)
			{
				output= "You see an " + NPCMoodDesc() + " " + playerUW.StringControl.GetString (7,npc_whoami+16);
			}
			else
			{
				output=output+" named " + playerUW.StringControl.GetString (7,npc_whoami+16);
			}

		}
		ml.Add (output);
		return true;
	}



	private string NPCMoodDesc()
	{
		//Gives a mood string for NPCs
		//004€005€096€hostile
			//004€005€097€upset
				//004€005€098€mellow
				//004€005€099€friendly
		switch (npc_attitude)
		{
		case 0:
			return playerUW.StringControl.GetString (5,96);break;
		case 1:
			return playerUW.StringControl.GetString (5,97);break;
		case 2:
			return playerUW.StringControl.GetString (5,98);break;
		default:
			return playerUW.StringControl.GetString (5,99);break;

		}
	}

}
