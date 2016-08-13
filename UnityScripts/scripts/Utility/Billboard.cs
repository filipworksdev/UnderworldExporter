﻿using UnityEngine;

/// <summary>
/// Billboard sprites so they always face the player.
/// </summary>
public class Billboard : MonoBehaviour
{
	//public float adjustment=0.0f;//0.8f;
		//public Vector3 adj;
	//private Quaternion dir;
	void Update()
	{
		//if (Camera.main!=null)
		//{
				//Objects will always be upright.
				//GameWorldController.instance.playerUW.gameObject.transform.position
				if (Vector3.Distance(this.transform.position, Camera.main.transform.position)<=8)
						{//Only rotate near objects.
								
							if (GameWorldController.instance.playerUW.dir!=Vector3.zero)
								{
								transform.rotation = Quaternion.LookRotation(GameWorldController.instance.playerUW.dir);						
								}
							
			//based on http://answers.unity3d.com/questions/524087/tweaking-sprite-billboard.html
			
						}

			//transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);


			//transform.rotation = Quaternion.LookRotation(GameWorldController.instance.playerUW.transform.forward-adj);
			//dir =Quaternion.LookRotation(transform.position - (Camera.main.transform.position - (Vector3.up*adjustment)));
			////////transform.rotation=Quaternion.LookRotation(transform.position - (Camera.main.transform.position - (Vector3.up*adjustment)));
				//dir;
			//transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
			//transform.rotation = new Quaternion(dir.x,dir.y,0.0f,dir.w);

		//}
		//transform.LookAt(Camera.main.transform.position, Vector3.up);
	}
}