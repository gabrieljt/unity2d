using System.Collections.Generic;
using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(Rigidbody2D),
	typeof(Collider2D)
)]
public class Character : MonoBehaviour
{
	[SerializeField]
	private float speed = 3f;

	private Stack<KeyCode> inputs = new Stack<KeyCode>();

	public bool HasInputs { get { return inputs.Count > 0; } }

	private SpriteRenderer spriteRenderer;

	new private Rigidbody2D rigidbody2D;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.freezeRotation = true;
	}

	private void Update()
	{
		direction = Vector2.zero;
		GetInputs();
	}

	private void FixedUpdate()
	{
		ProcessInputs();
	}

	private void GetInputs()
	{
		if (Input.GetKey(KeyCode.UpArrow))
		{
			inputs.Push(KeyCode.UpArrow);
		}

		if (Input.GetKey(KeyCode.DownArrow))
		{
			inputs.Push(KeyCode.DownArrow);
		}

		if (Input.GetKey(KeyCode.LeftArrow))
		{
			inputs.Push(KeyCode.LeftArrow);
		}

		if (Input.GetKey(KeyCode.RightArrow))
		{
			inputs.Push(KeyCode.RightArrow);
		}
	}

	private Vector2 direction = Vector2.zero;

	private void ProcessInputs()
	{
		while (HasInputs)
		{
			KeyCode input = inputs.Pop();
			switch (input)
			{
				case KeyCode.UpArrow:
					direction += Vector2.up;
					break;

				case KeyCode.DownArrow:
					direction += Vector2.down;
					break;

				case KeyCode.LeftArrow:
					direction += Vector2.left;
					break;

				case KeyCode.RightArrow:
					direction += Vector2.right;
					break;
			}
			spriteRenderer.flipX = direction.x > 0f;
		}

		rigidbody2D.MovePosition(rigidbody2D.position + (direction.normalized * speed * Time.fixedDeltaTime));
	}
}