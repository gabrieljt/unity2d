using System;
using UnityEngine;

public enum CharacterMovementState
{
	Idle,
	Moving,
	Collided,
}

[RequireComponent(
	typeof(Rigidbody2D),
	typeof(CircleCollider2D)
)]
public class CharacterMovement : MonoBehaviour
{
	[SerializeField]
	private CharacterMovementState state = CharacterMovementState.Idle;

	[SerializeField]
	private float speed;

	[SerializeField]
	private Vector2 previousDestination, destination;

	public Vector2 PreviousDestination { get { return previousDestination; } }


	private Rigidbody2D rigidbody2D;

	public bool ReachedDestination
	{
		get
		{
			return Vector2.Distance(destination, transform.position) <= 0.05f;
		}
	}

	public Action<Vector2> DestinationSet = delegate { };

	public Action DestinationReached = delegate { };

	public Action MovementStopped = delegate { };

	private bool CanMove
	{
		get
		{
			return state == CharacterMovementState.Idle;
		}
	}

	public Vector2 Position { get { return rigidbody2D.position; } }

	private void Awake()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		rigidbody2D.freezeRotation = true;
	}

	private void Update()
	{
		if (state == CharacterMovementState.Moving)
		{
			if (ReachedDestination)
			{
				StopMoving();
				DestinationReached();
				Debug.LogWarning("Destination Reached");
			}
		}
	}

	public void Move(Vector2 direction)
	{
		if (CanMove)
		{
			SetDestination(direction, out destination);
			DestinationSet(destination);
		}
	}

	private void SetDestination(Vector2 direction, out Vector2 destination)
	{
		state = CharacterMovementState.Moving;
		destination = Position + direction;

		Debug.Log("Moving to " + destination);
	}

	private void StopMoving()
	{
		MovementStopped();

		state = CharacterMovementState.Idle;
	}

	private void FixedUpdate()
	{
		if (state == CharacterMovementState.Moving)
		{
			MoveToDestination();
		}
	}

	private void MoveToDestination()
	{
		if (!ReachedDestination)
		{
			transform.position = Vector3.Lerp(transform.position, new Vector3(destination.x, destination.y, 0f), Mathf.Clamp(speed * 0.1f, 0.05f, 0.25f));
		}
		else
		{
			transform.position = destination;
		}
		return;
	}
}