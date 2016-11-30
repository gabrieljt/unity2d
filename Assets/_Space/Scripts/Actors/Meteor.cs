using System;
using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(MeteorMovement),
	typeof(Odometer)
)]
public class Meteor : MonoBehaviour, IDestroyable, IDisposable
{
	[SerializeField]
	private GameObject explosionPrefab;

	[SerializeField]
	private Odometer odometer;

	private Action<IDestroyable> destroyed = delegate { };

	public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

	private void Awake()
	{
		Debug.Assert(explosionPrefab);

		odometer = GetComponent<Odometer>();
		odometer.MaximumDistanceTravelled += OnMaximumDistanceTravelled;
		Debug.Assert(odometer.HasMaximumDistance);
	}

	private void Start()
	{
		var mass = GetComponent<MeteorMovement>().Mass;
		transform.localScale *= mass;
		odometer.maximumDistance = UnityEngine.Random.Range(odometer.maximumDistance, odometer.maximumDistance * mass);
	}

	private void OnMaximumDistanceTravelled()
	{
		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}

	public void OnDestroy()
	{
		Destroyed(this);
		Dispose();
	}

	public void Dispose()
	{
		odometer.MaximumDistanceTravelled -= OnMaximumDistanceTravelled;
	}
}