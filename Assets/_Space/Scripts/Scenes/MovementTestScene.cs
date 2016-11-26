using UnityEngine;
using System.Collections;

public class MovementTestScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PlayerInputEnqueuer.Instance.Add(FindObjectOfType<AActor>());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
