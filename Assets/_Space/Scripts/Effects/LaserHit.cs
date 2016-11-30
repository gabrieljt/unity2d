using UnityEngine;

public class LaserHit : MonoBehaviour
{
	[SerializeField]
	[Range(0.1f, 1f)]
	private float timeToDestroy = 0.2f;

	private void Start()
	{
		Destroy(gameObject, timeToDestroy);
	}
}