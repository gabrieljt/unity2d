/// <summary>
/// 2D Space Shooter Example
/// By Bug Games www.Bug-Games.net
/// Programmer: Danar Kayfi - Twitter: @DanarKayfi
/// Special Thanks to Kenney for the CC0 Graphic Assets: www.kenney.nl
/// 
/// This is the SharedValues Script:
/// - Shared Value Script between all other scripts
/// - In-Game & GameOver GUI
/// 
/// </summary>

using UnityEngine;
using System.Collections;

public class SharedValues_Script : MonoBehaviour 
{
	//Public Var
	public GUIText scoreText; 				//GUI Score
	public GUIText GameOverText; 			//GUI GameOver
	public GUIText FinalScoreText; 			//GUI Final Score
	public GUIText ReplayText; 				//GUI Replay

	//Public Shared Var
	public static int score = 0; 			//Total in-game Score
	public static bool gameover = false; 	//GameOver Trigger

	// Use this for initialization
	void Start () 
	{
		gameover = false; 					//return the Gameover trigger to its initial state when the game restart
		score = 0; 							//return the Score to its initial state when the game restart
	}

	// Fixed Update is called one per specific time
	void FixedUpdate ()
	{
		scoreText.text = "Score: " + score; 			//Update the GUI Score

		//Excute when the GameOver Trigger is True
		if (gameover == true)
		{
			GameOverText.text = "GAME OVER"; 			//Show GUI GameOver
			FinalScoreText.text = "" + score; 			//Show GUI FinalScore
			ReplayText.text = "PRESS R TO REPLAY"; 		//Show GUI Replay
		}
	}
}
