using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(LaserMovement)
)]
public class Laser : MonoBehaviour
{
	[SerializeField]
	private GameObject hitPrefab;

	private void Awake()
	{
		Debug.Assert(hitPrefab);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Instantiate(hitPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}
}