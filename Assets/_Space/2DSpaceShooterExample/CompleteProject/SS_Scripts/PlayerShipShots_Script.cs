/// <summary>
/// 2D Space Shooter Example
/// By Bug Games www.Bug-Games.net
/// Programmer: Danar Kayfi - Twitter: @DanarKayfi
/// Special Thanks to Kenney for the CC0 Graphic Assets: www.kenney.nl
/// 
/// This is the PlayerShipShots Script:
/// - Ship Shot velocity
/// 
/// </summary>

using UnityEngine;
using System.Collections;

public class PlayerShipShots_Script : MonoBehaviour 
{
	//Public Var
	public float speed; //Speed of the velocity
	
	// Use this for initialization
	void Start ()
	{
		GetComponent<Rigidbody2D>().velocity = transform.up * speed; //Give Velocity to the Player ship shot
	}
}
