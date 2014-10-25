﻿using UnityEngine;
using System.Collections;

public class Magic : MonoBehaviour {
	static string[] Runes=new string[]{"An","Bet","Corp","Des",
								"Ex","Flam","Grav","Hur",
								"In","Jux","Kal","Lor",
								"Mani","Nox","Ort","Por",
								"Quas","Rel","Sanct","Tym",
								"Uus","Vas","Wis","Ylem"};
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	static public void castSpell(int Rune1, int Rune2, int Rune3)
	{
		string MagicWords="";
		//Construct the spell words based on selected runes
		if ((Rune1>=0) && (Rune1<=23))
		{
			MagicWords= Runes[Rune1];
		}
		if ((Rune2>=0) && (Rune2<=23))
		{
			MagicWords=MagicWords + " " + Runes[Rune2];
		}
		if ((Rune3>=0) && (Rune3<=23))
		{
			MagicWords=MagicWords + " " + Runes[Rune3];
		}

		switch (MagicWords)
		{
			//1st Circle
		case "In Mani Ylem"://Create Food
			{
			Debug.Log(MagicWords+ " Create Food Cast");
			break;
			}//imy
		case "In Lor":	//Light
			{
			Debug.Log(MagicWords+ " Light Cast");
			break;
			}	//il
		case "Bet Wis Ex"://Locate
			{
			Debug.Log(MagicWords+ " Locate Cast");
			break;
			}//bwe
		case "Ort Jux"://Magic Arrow
			{
			Debug.Log(MagicWords+ " Magic Arrow Cast");
			break;
			}//OJ
		case "Bet In Sanct"://Resist Blows
			{
			Debug.Log(MagicWords+ " Resist Blows Cast");
			break;
			}//BIS
		case "Sanct Hur"://Stealth
			{
			Debug.Log(MagicWords+ " Stealth Cast");
			break;
			}//sh
		
		//2nd Circle
		case "Quas Corp"://Cause Fear
			{
			Debug.Log(MagicWords+ " Cause Fear Cast");
			break;
			}//qc
		case "Wis Mani"://Detect Monster
			{
			Debug.Log(MagicWords+ " Detect Monster Cast");
			break;
			}//wm
		case "Uus Por"://Jump
			{
			Debug.Log(MagicWords+ " Jump Cast");
			break;
			}//up
		case "In Bet Mani"://Lesser Heal
			{
			Debug.Log(MagicWords+ " Lesser Heal Cast");
			break;
			}//IBM
		case "Rel Des Por"://Slow Fall
			{
			Debug.Log(MagicWords+ " Slow Fall Cast");
			break;
			}//RDP
		case "In Sanct"://Thick Skin
			{
			Debug.Log(MagicWords+ " Thick Skin Cast");
			break;
			}//IS
		case "In Jux"://Rune of Warding
			{
			Debug.Log(MagicWords+ " Rune of Warding Cast");
			break;
			}//IJ

		//3rd Circle
		case "Bet Sanct Lor"://Conceal
			{
			Debug.Log(MagicWords+ " Conceal Cast");
			break;
			}//BSL
		case "Ort Grav"://Lightning
			{
			Debug.Log(MagicWords+ " Lightning Cast");
			break;
			}//OG
		case "Quas Lor"://Night Vision
			{
			Debug.Log(MagicWords+ " Night Vision Cast");
			break;
			}//QL
		case "An Kal Corp"://Repel Undead
			{
			Debug.Log(MagicWords+ " Repel Undead Cast");
			break;
			}//akc
		case "Rel Tym Por"://Speed
			{
			Debug.Log(MagicWords+ " Speed Cast");
			break;
			}//rtp
		case "Ylem Por"://Water Walk
			{
			Debug.Log(MagicWords+ " Water Walk Cast");
			break;
			}//YP
		case "Sanct Jux"://Strengten Door
			{
			Debug.Log(MagicWords+ " Strengten Door Cast");
			break;
			}//SJ
		
		//4th Circle
		case "An Sanct"://Curse
			{
			Debug.Log(MagicWords+ " Curse Cast");
			break;
			}//AS
		case "Sanct Flam":// Flameproof
			{
			Debug.Log(MagicWords+ " Flameproof Cast");
			break;
			}//SF
		case "In Mani"://Heal
			{
			Debug.Log(MagicWords+ " Heal Cast");
			break;
			}//IM
		case "Hur Por"://Levitate
			{	
			Debug.Log(MagicWords+ " Cast");
			break;
			}//HP
		case "Nox Ylem"://Poison
			{
			Debug.Log(MagicWords+ " Poison Cast");
			break;
			}//NY
		case "An Jux"://Remove Trap
			{
			Debug.Log(MagicWords+ " Remove Trap Cast");
			break;
			}//AJ

		//5th Circle
		case "Por Flam"://Fireball
			{
			Debug.Log(MagicWords+ " Fireball Cast");
			break;
			}//PF
		case "Grav Sanct Por"://Missile Protection
			{
			Debug.Log(MagicWords+ " Missile Protection Cast");
			break;
			}//GSP
		case "Ort Wis Ylem"://Name Enchantment
			{
			Debug.Log(MagicWords+ " Name Enchantment Cast");
			break;
			}//OWY
		case "Ex Ylem"://Open
			{
			Debug.Log(MagicWords+ " Open Cast");
			break;
			}//EY
		case "An Nox"://Cure Poison
			{
			Debug.Log(MagicWords+ " Cure Poison Cast");
			break;
			}//AN
		case "An Corp Mani"://Smite Undead
			{
			Debug.Log(MagicWords+ " Smite Undead Cast");
			break;
			}//ACM
		
			//6th Circle
		case "Vas In Lor"://Daylight
			{
			Debug.Log(MagicWords+ " Daylight Cast");
			break;
			}//VIL
		case "Vas Rel Por"://Gate Travel
			{
			Debug.Log(MagicWords+ " Gate Travel Cast");
			break;
			}//VRP
		case "Vas In Mani"://Greater Heal
			{
			Debug.Log(MagicWords+ " Greater Heal Cast");
			break;
			}//VIM
		case "An Ex Por"://Paralyze
			{
			Debug.Log(MagicWords+ " Paralyze Cast");
			break;
			}//AEP
		case "Vas Ort Grav"://Sheet Lightning
			{
			Debug.Log(MagicWords+ " Sheet Lightning Cast");
			break;
			}//VOG
		case "Ort Por Ylem"://Telekinesis
			{
			Debug.Log(MagicWords+ " Telekinesis Cast");
			break;
			}//OPY

		//7th Circle
		case "In Mani Rel"://Ally
			{
			Debug.Log(MagicWords+ " Ally Cast");
			break;
			}//IMR
		case "Vas An Wis"://Confusion
			{
			Debug.Log(MagicWords+ " Confusion Cast");
			break;
			}//VAW
		case "Vas Sanct Lor"://Invisibility
			{
			Debug.Log(MagicWords+ " Invisibility Cast");
			break;
			}//VSL
		case "Vas Hur Por"://Fly
			{
			Debug.Log(MagicWords+ " Fly Cast");
			break;
			}//VHP
		case "Kal Mani"://Monster Summoning
			{
			Debug.Log(MagicWords+ " Monster Summoning Cast");
			break;
			}//KM
		case "Ort An Quas"://Reveal
			{
			Debug.Log(MagicWords+ " Reveal Cast");
			break;
			}//OAQ
		//8th Circle
		case "Vas Kal Corp"://Armageddon
			{
			Debug.Log(MagicWords+ " Armageddon Cast");
			break;
			}//vkc
		case "Flam Hur"://Flame Wind
			{
			Debug.Log(MagicWords+ " Flame Wind Cast");
			break;
			}//fh
		case "An Tym":// Freeze Time
			{
			Debug.Log(MagicWords+ " Freeze Time Cast");
			break;
			}//at
		case "In Vas Sanct"://Iron Flesh
			{
			Debug.Log(MagicWords+ " Iron Flesh Cast");
			break;
			}//ivs
		case "Ort Por Wis"://Roaming sight
			{
			Debug.Log(MagicWords+ " Roaming sight Cast");
			break;
			}//opw
		case "Vas Por Ylem"://Tremor
			{
			Debug.Log(MagicWords+ " Tremor Cast");
			break;
			}//vpy
		default:
			{
			Debug.Log("Unknown spell:" + MagicWords);
			break;
			}
		}//magicwords
	}
}
