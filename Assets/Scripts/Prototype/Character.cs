using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
public class Character : MonoBehaviour
{
	[SerializeField]
	private CharacterState state = CharacterState.Idle;

	[SerializeField]
	private int steps = 0;

	public int Steps { get { return steps; } }

	[SerializeField]
	[Range(0.5f, 3f)]
	private float speed = 3f;

	private Queue<KeyCode> inputs = new Queue<KeyCode>();

	public bool HasInputs { get { return inputs.Count > 0; } }

	[SerializeField]
	[Range(0, 10)]
	private int maximumInputsPerFrame = 1;

	public int MaximumInputsPerFrame { get; private set; }

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

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		rigidbody2D.freezeRotation = true;

		MaximumInputsPerFrame = maximumInputsPerFrame;
	}

	private void Update()
	{
		GetInput();
		ProcessInputs();
		UpdateMovementState();
	}

	private void FixedUpdate()
	{
		MoveToDestination();
	}

	private void GetInput()
	{
		if (Input.anyKey && inputs.Count < maximumInputsPerFrame)
		{
			if (Input.GetKey(KeyCode.UpArrow))
			{
				inputs.Enqueue(KeyCode.UpArrow);
				return;
			}

			if (Input.GetKey(KeyCode.DownArrow))
			{
				inputs.Enqueue(KeyCode.DownArrow);
				return;
			}

			if (Input.GetKey(KeyCode.LeftArrow))
			{
				inputs.Enqueue(KeyCode.LeftArrow);
				return;
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				inputs.Enqueue(KeyCode.RightArrow);
				return;
			}
		}
	}

	private void ProcessInputs()
	{
		if (state == CharacterState.Idle)
		{
			if (HasInputs)
			{
				Vector2 direction = Vector2.zero;
				KeyCode input = inputs.Dequeue();
				switch (input)
				{
					case KeyCode.UpArrow:
						direction = Vector2.up;
						break;

					case KeyCode.DownArrow:
						direction = Vector2.down;
						break;

					case KeyCode.LeftArrow:
						direction = Vector2.left;
						break;

					case KeyCode.RightArrow:
						direction = Vector2.right;
						break;
				}

				SetDestination(direction);
			}
		}
	}

	private void SetDestination(Vector2 direction)
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
		inputs.Clear();
		state = CharacterState.Idle;
	}

	private void CollisionFallback()
	{
		destination = previousDestination;
		inputs.Clear();
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

	public void Enable()
	{
		gameObject.SetActive(true);
		if (Application.isPlaying)

		{
			rigidbody2D.isKinematic = false;
		}

		StartCoroutine(UnlockInputsCoroutine());
	}

	private IEnumerator UnlockInputsCoroutine()
	{
		int targetFrameRate = Application.targetFrameRate == 0 ? 60 : Application.targetFrameRate;
		yield return new WaitForSeconds(1f / targetFrameRate * 10f);

		maximumInputsPerFrame = MaximumInputsPerFrame;
	}

	public void Disable()
	{
		HaltMovement();
		LockInputs();

		if (Application.isPlaying)
		{
			rigidbody2D.isKinematic = true;
		}

		gameObject.SetActive(false);
	}

	private void LockInputs()
	{
		maximumInputsPerFrame = 0;
	}
}