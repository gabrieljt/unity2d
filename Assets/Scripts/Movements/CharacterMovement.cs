using System;
using UnityEngine;

public enum CharacterMovementState
{
	Idle,
	Moving,
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
	[Range(1f, 10f)]
	private float speed;

	[SerializeField]
	private Vector2 previousDestination, destination, direction;

	[SerializeField]
	private Rigidbody2D rigidbody2D;

	private bool CanMove { get { return state == CharacterMovementState.Idle; } }

	public Vector2 PreviousDestination { get { return previousDestination; } }

	public Vector2 Direction { get { return direction; } }

	public float DestinationReachedError
	{
		get
		{
			return Mathf.Clamp(speed * 0.01f, 0.025f, 0.1f);
		}
	}

	public bool ReachedDestination { get { return Vector2.Distance(destination, transform.position) <= DestinationReachedError; } }

	public Vector2 Position { get { return rigidbody2D.position; } }

	public Vector2 Velocity { get { return ((destination - Position).normalized * speed); } }

	public Action<Vector2, Vector2> DestinationSet = delegate { };

	public Action DestinationReached = delegate { };

	public Action MovementStopped = delegate { };

	private Action<MonoBehaviour> destroyed = delegate { };

	public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

	public float DistanceToDestination { get { return Vector2.Distance(Position, destination); } }
	public float DistanceToPreviousDestination { get { return Vector2.Distance(Position, previousDestination); } }

	public bool FallingBack { get { return previousDestination == destination; } }

	private void Awake()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rigidbody2D.freezeRotation = true;
	}

	private void Start()
	{
		previousDestination = destination = transform.position;
		direction = Vector2.zero;
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
			}
		}
	}

	public void Move(Vector2 direction)
	{
		if (CanMove)
		{
			this.direction = direction;
			SetDestination(direction, out previousDestination, out destination);
			DestinationSet(destination, direction);
		}
	}

	private void SetDestination(Vector2 direction, out Vector2 previousDestination, out Vector2 destination)
	{
		state = CharacterMovementState.Moving;
		previousDestination = Position;
		destination = Position + direction;
	}

	private void StopMoving()
	{
		state = CharacterMovementState.Idle;
		rigidbody2D.velocity = Vector2.zero;
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
			rigidbody2D.MovePosition(Position + Velocity * Time.fixedDeltaTime);
		}
	}

	private void FallBack()
	{
		if (!FallingBack)
		{
			direction *= -1;
			destination = previousDestination;
			DestinationSet(destination, direction);
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		Collided(other);
	}

	private void Collided(Collision2D other)
	{
		if (state == CharacterMovementState.Moving)
		{
			var otherMovement = other.gameObject.GetComponent<CharacterMovement>();

			// Wall Collision, just fallback.
			if (!otherMovement)
			{
				FallBack();
				return;
			}

			// Other is Holding ground, fallback.
			if (otherMovement.state == CharacterMovementState.Idle)
			{
				otherMovement.StopMoving();
				FallBack();
				return;
			}

			if (otherMovement.state == CharacterMovementState.Moving)
			{
				// Other holded ground, fallback.
				if (otherMovement.FallingBack)
				{
					if (DistanceToDestination >= DistanceToPreviousDestination)
					{
						FallBack();
						return;
					}
				}

				// Ground control
				if (destination == otherMovement.destination)
				{
					// Other is closer to center, fallback.
					if (DistanceToDestination >= otherMovement.DistanceToDestination)
					{
						FallBack();
						return;
					}
				}

				// Hit and fallback. theoreticallyShouldBe(faster || collider.GreaterThan(otherCollider));
				if (destination == otherMovement.previousDestination)
				{
					FallBack();
					return;
				}
			}
		}
	}

	private static Vector2 RandomDirection()
	{
		var direction = Vector2.zero;
		var dice = UnityEngine.Random.Range(0, 4);
		if (dice == 0)
		{
			direction = Vector2.up;
		}

		if (dice == 1)
		{
			direction = Vector2.down;
		}

		if (dice == 2)
		{
			direction = Vector2.left;
		}

		if (dice == 3)
		{
			direction = Vector2.right;
		}

		return direction;
	}
}