using System;
using UnityEngine;

public enum CharacterState
{
	Idle,
	Moving,
	FallingBack,
}

[RequireComponent(
	typeof(CharacterMovement),
	typeof(SpriteRenderer),
	typeof(Rigidbody2D)
//typeof(CircleCollider2D)
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

	private SpriteRenderer spriteRenderer;

	private Rigidbody2D rigidbody2D;

	public Action StepTaken = delegate { };

	[SerializeField]
	private CharacterMovement characterMovement;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		rigidbody2D.freezeRotation = true;

		characterMovement = GetComponent<CharacterMovement>();
		characterMovement.DestinationSet += OnDestinationSet;
		characterMovement.DestinationReached += OnDestinationReached;
	}

	private void OnDestinationSet(Vector2 destination)
	{
		spriteRenderer.flipX = (destination - characterMovement.Position).x > 0f;
	}

	private void OnDestinationReached()
	{
		if (characterMovement.PreviousDestination != characterMovement.Position)
		{
			steps++;
			StepTaken();
		}
	}

	private void Start()
	{
	}

	private void UpdateMovementState()
	{
		/*if (state == CharacterState.FallingBack)
		{
			if (ReachedDestination)
			{
				Debug.LogWarning("Destination Reached from fallback");
				return;
			}
		}*/
	}

	/*private void CollisionFallback()
	{
		destination = characterMovement.PreviousDestination;

		MovementHalted();

		state = CharacterState.FallingBack;

		Debug.Log("Collided: falling back to " + destination);
	}*/

	private void OnCollisionEnter2D()
	{
		//CollisionFallback();
	}

	private void OnCollisionStay2D()
	{
		if (state != CharacterState.FallingBack)
		{
			//CollisionFallback();

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