using System;
using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(LaserMovement),
	typeof(Odometer)
)]
public class Laser : MonoBehaviour, IDestroyable, IDisposable
{
	[SerializeField]
	private GameObject hitPrefab;

	[SerializeField]
	private Odometer odometer;

	private Action<IDestroyable> destroyed = delegate { };

	public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

	private void Awake()
	{
		Debug.Assert(hitPrefab);

		odometer = GetComponent<Odometer>();
		odometer.MaximumDistanceTravelled += OnMaximumDistanceTravelled;
		Debug.Assert(odometer.HasMaximumDistance);
	}

	private void OnMaximumDistanceTravelled()
	{
		Instantiate(hitPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Instantiate(hitPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}

	public void OnDestroy()
	{
		Destroyed(this);
		Dispose();
	}

	public void Dispose()
	{
		odometer.Travelled -= OnMaximumDistanceTravelled;
	}
}