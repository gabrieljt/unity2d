using UnityEngine;

public class SpaceshipTestScene : MonoBehaviour
{
	private void Start()
	{
		var spaceship = FindObjectOfType<Spaceship>();
		PlayerInputEnqueuer.Instance.Add(spaceship.gameObject);
		Camera.main.GetComponent<FollowCamera>().target = spaceship.transform;
	}
}