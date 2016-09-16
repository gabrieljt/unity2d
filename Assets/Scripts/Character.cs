using System.Collections.Generic;
using UnityEngine;

public enum CharacterState
{
	Idle,
	Moving
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

	[SerializeField]
	[Range(0.5f, 3f)]
	private float speed = 3f;

	private Vector2 direction = Vector2.zero;

	private Queue<KeyCode> inputs = new Queue<KeyCode>();

	public Queue<KeyCode> Inputs { get { return inputs; } }

	public bool HasInputs { get { return inputs.Count > 0; } }

	public bool ReachedDestination
	{
		get
		{
			return Vector2.Distance(destination, transform.position) <= 0.05f;
		}
	}

	private SpriteRenderer spriteRenderer;

	private Rigidbody2D rigidbody2D;

	public Rigidbody2D Rigidbody2D { get { return rigidbody2D; } }

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rigidbody2D.freezeRotation = true;
	}

	private void Update()
	{
		GetInput();
		ProcessInputs();
		Move();
	}

	private void GetInput()
	{
		if (Input.anyKey && inputs.Count < 1)
		{
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				inputs.Enqueue(KeyCode.UpArrow);
				return;
			}

			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				inputs.Enqueue(KeyCode.DownArrow);
				return;
			}

			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				inputs.Enqueue(KeyCode.LeftArrow);
				return;
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				inputs.Enqueue(KeyCode.RightArrow);
				return;
			}
		}
	}

	[SerializeField]
	private Vector2 previousDestination, destination;

	private bool collided = false;

	private void ProcessInputs()
	{
		if (state == CharacterState.Idle)
		{
			if (HasInputs)
			{
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

				spriteRenderer.flipX = direction.x > 0f;

				previousDestination = new Vector2(transform.position.x, transform.position.y);
				destination = previousDestination + direction;

				state = CharacterState.Moving;

				Debug.Log("Moving to " + destination);
			}
		}
	}

	private void Move()
	{
		if (state == CharacterState.Moving)
		{
			if (ReachedDestination)
			{
				if (!collided)
				{
					steps++;
				}

				HaltMovement();

				Debug.LogWarning("Destination Reached");
				return;
			}

			if (!ReachedDestination || collided)
			{
				transform.position = Vector3.Lerp(transform.position, new Vector3(destination.x, destination.y, 0f), Mathf.Clamp(speed * 0.1f, 0.05f, 0.25f));
			}
		}
	}

	public void HaltMovement()
	{
		collided = false;
		direction = Vector2.zero;
		transform.position = destination;
		state = CharacterState.Idle;
	}

	private void OnCollisionEnter2D()
	{
		collided = true;
		destination = previousDestination;
		inputs.Clear();

		Debug.Log("Collided: returning to " + destination);
	}
}