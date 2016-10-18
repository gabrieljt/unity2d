using System;
using UnityEngine;

public enum CharacterMovementState
{
	Idle,
	Moving,
}

[RequireComponent(
	typeof(Character),
	typeof(Rigidbody2D),
	typeof(CircleCollider2D)
)]
public class CharacterMovement : MonoBehaviour, IDisposable, IDestroyable
{
	[SerializeField]
	private CharacterMovementState state = CharacterMovementState.Idle;

	[SerializeField]
	[Range(1, 50)]
	private int speed;

	[SerializeField]
	private Vector2 previousDestination, destination;

	public Vector2 PreviousDestination { get { return previousDestination; } }

	[SerializeField]
	private Character character;

	[SerializeField]
	private Rigidbody2D rigidbody2D;

	public bool ReachedDestination
	{
		get
		{
			return Vector2.Distance(destination, transform.position) <= 0.025f;
		}
	}

	private bool CanMove
	{
		get
		{
			return state == CharacterMovementState.Idle;
		}
	}

	public Vector2 Position { get { return rigidbody2D.position; } }

	public Action<Vector2> DestinationSet = delegate { };

	public Action DestinationReached = delegate { };

	public Action MovementStopped = delegate { };

	private Action<MonoBehaviour> destroyed = delegate { };

	public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

	public float DistanceToDestination { get { return Vector2.Distance(Position, destination); } }

	private void Awake()
	{
		character = GetComponent<Character>();
		character.Enabled += OnCharacterEnabled;
		character.Disabled += OnCharacterDisabled;

		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		rigidbody2D.freezeRotation = true;
	}

	private void Start()
	{
		previousDestination = destination = transform.position;
	}

	private void Update()
	{
		if (state == CharacterMovementState.Moving)
		{
			if (ReachedDestination)
			{
				StopMoving();
				MovementStopped();
				DestinationReached();
				Debug.LogWarning("Destination Reached");
			}
		}
	}

	public void Move(Vector2 direction)
	{
		if (CanMove)
		{
			SetDestination(direction, out previousDestination, out destination);
			DestinationSet(destination);
		}
	}

	private void SetDestination(Vector2 direction, out Vector2 previousDestination, out Vector2 destination)
	{
		state = CharacterMovementState.Moving;
		previousDestination = Position;
		destination = Position + direction;

		Debug.Log("Moving to " + destination);
	}

	private void StopMoving()
	{
		state = CharacterMovementState.Idle;
		transform.position = destination;
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
			transform.position = Vector3.Lerp(transform.position, new Vector3(destination.x, destination.y, 0f), Mathf.Clamp(speed * 0.01f, 0.01f, 0.50f));
		}
	}

	private void FallBack()
	{
		destination = previousDestination;
		Debug.Log("Falling back to " + previousDestination);
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		Collided(other);
	}

	private void OnCollisionStay2D(Collision2D other)
	{
		Collided(other);
	}

	private void Collided(Collision2D other)
	{
		if (state == CharacterMovementState.Moving)
		{
			var otherCharacterMovement = other.gameObject.GetComponent<CharacterMovement>();

			if (!otherCharacterMovement)
			{
				FallBack();
				return;
			}

			if (otherCharacterMovement.state == CharacterMovementState.Idle)
			{
				FallBack();
				otherCharacterMovement.StopMoving();
				return;
			}

			if (otherCharacterMovement.state == CharacterMovementState.Moving)
			{
				if (destination == otherCharacterMovement.destination)
				{
					if (DistanceToDestination >= otherCharacterMovement.DistanceToDestination)
					{
						FallBack();
						return;
					}
				}

				if (destination == otherCharacterMovement.previousDestination)
				{
					FallBack();
					return;
				}
			}
		}
	}

	private void OnCharacterEnabled(AActor character)
	{
		rigidbody2D.isKinematic = true;
	}

	private void OnCharacterDisabled(AActor character)
	{
		rigidbody2D.isKinematic = false;
	}

	public void Dispose()
	{
		character.Enabled -= OnCharacterEnabled;
		character.Disabled -= OnCharacterDisabled;
	}

	public void OnDestroy()
	{
		Dispose();
	}
}