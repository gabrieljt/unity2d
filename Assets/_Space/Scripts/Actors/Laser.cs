using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(LaserMovement)
)]
public class Laser : AActor
{
	[SerializeField]
	private SpriteRenderer renderer;

	[SerializeField]
	private LaserMovement movement;

	[SerializeField]
	private GameObject hitPrefab;

	private void Awake()
	{
		//Debug.Assert(hitPrefab);

		renderer = GetComponent<SpriteRenderer>();
		movement = GetComponent<LaserMovement>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//Instantiate(hitPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}

	public override void Dispose()
	{
	}
}