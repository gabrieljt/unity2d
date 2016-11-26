using UnityEngine;

public class MovementTestScene : MonoBehaviour
{
	// Use this for initialization
	private void Start()
	{
		var spaceship = FindObjectOfType<Spaceship>();
		PlayerInputEnqueuer.Instance.Add(spaceship);
		FollowCamera.Instance.target = spaceship.transform;
	}

	// Update is called once per frame
	private void Update()
	{
	}
}