using System;
using UnityEngine;

namespace Game.Actor
{
	public enum CharacterState
	{
		Idle,
		Moving,
		FallingBack,
	}

	[RequireComponent(
		typeof(SpriteRenderer),
		typeof(Rigidbody2D),
		typeof(CircleCollider2D)
	)]
	public class Character : AActor
	{
		[SerializeField]
		private CharacterState state = CharacterState.Idle;

		public CharacterState State { get { return state; } }

		[SerializeField]
		private int steps = 0;

		public int Steps { get { return steps; } }

		[SerializeField]
		[Range(0.5f, 3f)]
		private float speed = 3f;

		[SerializeField]
		private Vector2 previousDestination, destination;

		public bool ReachedDestination
		{
			get
			{
				return Vector2.Distance(destination, transform.position) <= 0.05f;
			}
		}

		private SpriteRenderer spriteRenderer;

		private Rigidbody2D rigidbody2D;

		public Action StepTaken = delegate { };

		public Action MovementHalted = delegate { };

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();

			rigidbody2D = GetComponent<Rigidbody2D>();
			rigidbody2D.gravityScale = 0f;
			rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
			rigidbody2D.freezeRotation = true;
		}

		private void Update()
		{
			UpdateMovementState();
		}

		private void FixedUpdate()
		{
			MoveToDestination();
		}

		public void SetDestination(Vector2 direction)
		{
			spriteRenderer.flipX = direction.x > 0f;

			previousDestination = new Vector2(transform.position.x, transform.position.y);
			destination = previousDestination + direction;

			state = CharacterState.Moving;

			Debug.Log("Moving to " + destination);
		}

		private void UpdateMovementState()
		{
			if (state == CharacterState.FallingBack)
			{
				if (ReachedDestination)
				{
					HaltMovement();

					Debug.LogWarning("Destination Reached from fallback");
					return;
				}
			}

			if (state == CharacterState.Moving)
			{
				if (ReachedDestination)
				{
					steps++;
					StepTaken();

					HaltMovement();

					Debug.LogWarning("Destination Reached");
				}
			}
		}

		private void MoveToDestination()
		{
			if (state != CharacterState.Idle)
			{
				Move();
				return;
			}
		}

		private void Move()
		{
			if (!ReachedDestination)
			{
				transform.position = Vector3.Lerp(transform.position, new Vector3(destination.x, destination.y, 0f), Mathf.Clamp(speed * 0.1f, 0.05f, 0.25f));
			}
		}

		private void HaltMovement()
		{
			transform.position = destination;

			MovementHalted();

			state = CharacterState.Idle;
		}

		private void CollisionFallback()
		{
			destination = previousDestination;

			MovementHalted();

			state = CharacterState.FallingBack;

			Debug.Log("Collided: falling back to " + destination);
		}

		private void OnCollisionEnter2D()
		{
			CollisionFallback();
		}

		private void OnCollisionStay2D()
		{
			if (state != CharacterState.FallingBack)
			{
				CollisionFallback();

				Debug.LogWarning("CollisionStay Fallback");
			}
		}

		public override void Enable()
		{
			base.Enable();
			gameObject.SetActive(true);
			if (Application.isPlaying)

			{
				rigidbody2D.isKinematic = false;
			}
		}

		public override void Disable()
		{
			base.Disable();

			if (Application.isPlaying)
			{
				rigidbody2D.isKinematic = true;
			}

			gameObject.SetActive(false);
		}

		public override void Dispose()
		{
		}
	}
}