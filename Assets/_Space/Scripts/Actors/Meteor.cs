using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(MeteorMovement)
)]
public class Meteor : AActor
{
	[SerializeField]
	private SpriteRenderer renderer;

	[SerializeField]
	private MeteorMovement movement;

	[SerializeField]
	private GameObject explosionPrefab;

	private void Awake()
	{
		renderer = GetComponent<SpriteRenderer>();
		movement = GetComponent<MeteorMovement>();
		Debug.Assert(explosionPrefab);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}

	public override void Dispose()
	{
	}
}