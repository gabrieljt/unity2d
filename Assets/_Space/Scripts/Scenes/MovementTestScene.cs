using UnityEngine;

public class MovementTestScene : MonoBehaviour
{
	// Use this for initialization
	private void Start()
	{
		PlayerInputEnqueuer.Instance.Add(FindObjectOfType<AActor>());
	}

	// Update is called once per frame
	private void Update()
	{
	}
}