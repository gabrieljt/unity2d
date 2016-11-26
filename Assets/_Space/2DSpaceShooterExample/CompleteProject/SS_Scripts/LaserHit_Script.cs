/// <summary>
/// 2D Space Shooter Example
/// By Bug Games www.Bug-Games.net
/// Programmer: Danar Kayfi - Twitter: @DanarKayfi
/// Special Thanks to Kenney for the CC0 Graphic Assets: www.kenney.nl
/// 
/// This is the LaserHit Script:
/// - Destroy the LaserHit after specific time
/// 
/// </summary>

using UnityEngine;
using System.Collections;

public class LaserHit_Script : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		Destroy(gameObject,0.05f); //Destroy the object after specific time
	}
}
