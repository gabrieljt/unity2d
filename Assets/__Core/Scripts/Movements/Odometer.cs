using System;
using UnityEngine;

[RequireComponent(
	typeof(IMoveable)
)]
public class Odometer : MonoBehaviour
{
	public float maximumDistance = float.MaxValue;

	[SerializeField]
	private IMoveable movement;

	[SerializeField]
	private Vector2 previousPosition;

	[SerializeField]
	private float distanceTravelled = 0;

	public float DistanceTravelled { get { return distanceTravelled; } }

	public Vector2 PreviousPositionDirection { get { return (previousPosition - movement.Position).normalized; } }

	public float DistanceToPreviousPosition { get { return Vector2.Distance(movement.Position, previousPosition); } }

	public bool HasMaximumDistance { get { return maximumDistance != float.MaxValue; } }

	public Action Travelled = delegate { };

	public Action MaximumDistanceTravelled = delegate { };

	private void Awake()
	{
		movement = GetComponent<IMoveable>();
	}

	private void Start()
	{
		previousPosition = movement.Position;
	}

	private void FixedUpdate()
	{
		distanceTravelled += DistanceToPreviousPosition;
		Debug.DrawRay(movement.Position, PreviousPositionDirection * DistanceToPreviousPosition, Color.red, 3f);
		previousPosition = movement.Position;
		Travelled();

		if (HasMaximumDistance && distanceTravelled >= maximumDistance)
		{
			MaximumDistanceTravelled();
		}
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			var gizmosColor = Gizmos.color;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(previousPosition, DistanceToPreviousPosition);

			Debug.DrawRay(movement.Position, PreviousPositionDirection * DistanceToPreviousPosition, Color.green, 1f);

			Gizmos.color = gizmosColor;
		}
	}

#endif
}