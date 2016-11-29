using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(MeteorMovement)
)]
public class Meteor : MonoBehaviour
{
	[SerializeField]
	private GameObject explosionPrefab;

	[SerializeField]
	private MeteorMovement movement;

	private void Awake()
	{
		movement = GetComponent<MeteorMovement>();
		Debug.Assert(explosionPrefab);
	}

	private void Start()
	{
		transform.localScale *= movement.Mass;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}
}