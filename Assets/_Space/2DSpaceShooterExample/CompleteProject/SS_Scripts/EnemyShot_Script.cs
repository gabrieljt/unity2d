/// <summary>
/// 2D Space Shooter Example
/// By Bug Games www.Bug-Games.net
/// Programmer: Danar Kayfi - Twitter: @DanarKayfi
/// Special Thanks to Kenney for the CC0 Graphic Assets: www.kenney.nl
///
/// This is the EnemyShot Script:
/// - Enemy Ship Shot velocity
///
/// </summary>

using UnityEngine;

public class EnemyShot_Script : MonoBehaviour
{
	//Public Var
	public float speed; //EnemyRed Shot Speed

	// Use this for initialization
	private void Start()
	{
		GetComponent<Rigidbody2D>().velocity = -1 * transform.up * speed; //Give Velocity to the Enemy ship shot
	}
}