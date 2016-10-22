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
	[Range(1f, 10f)]
	private float speed;

	[SerializeField]
	private Vector2 direction;

	public Vector2 Direction { get { return direction; } }

	[SerializeField]
	private Rigidbody2D rigidbody;

	[SerializeField]
	private CircleCollider2D collider;

	public Vector2 Position { get { return rigidbody.position; } }

	public Vector2 Velocity { get { return direction * speed; } }

	public Action<Vector2> Moving = delegate { };

	public Action Stopped = delegate { };
	public Action Collided = delegate { };

	private Action<MonoBehaviour> destroyed = delegate { };

	public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		rigidbody.gravityScale = 0f;
		rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rigidbody.freezeRotation = true;

		collider = GetComponent<CircleCollider2D>();
		if (!collider.sharedMaterial)
		{
			Debug.LogWarning(name + " physics material not set.");
		}
	}

	private void Start()
	{
		direction = Vector2.zero;
	}

	public void Move(Vector2 direction)
	{
		this.direction = direction;
		Moving(direction);
	}

	private void FixedUpdate()
	{
		MoveToDestination();
	}

	private void MoveToDestination()
	{
		rigidbody.MovePosition(Position + Velocity * Time.fixedDeltaTime);
	}
}