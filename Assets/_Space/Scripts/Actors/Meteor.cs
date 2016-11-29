using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(MeteorMovement)
)]
public class Meteor : MonoBehaviour
{
	[SerializeField]
	private GameObject explosionPrefab;

	private void Awake()
	{
		Debug.Assert(explosionPrefab);
	}

	private void Start()
	{
		transform.localScale *= GetComponent<MeteorMovement>().Mass;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}
}