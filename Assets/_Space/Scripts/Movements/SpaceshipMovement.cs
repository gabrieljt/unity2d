using System;
using UnityEngine;

[RequireComponent(
	typeof(Rigidbody2D)
)]
public class SpaceshipMovement : MonoBehaviour, IMoveable, ISteerable
{
	[SerializeField]
	private Rigidbody2D rigidbody;

	[SerializeField]
	[Range(1f, 100f)]
	private float speed;

	public float Speed { get { return speed; } }

	[SerializeField]
	private Vector2 direction;

	public Vector2 Direction { get { return direction; } }

	public Vector2 Position { get { return rigidbody.position; } }

	public Vector2 Velocity
	{
		get
		{
			if (direction == Vector2.up)
			{
				return transform.up * speed;
			}
			else if (direction == Vector2.down)
			{
				return -transform.up * speed;
			}

			return Vector2.zero;
		}
	}

	private Action<Vector2> moving = delegate { };

	public Action<Vector2> Moving { get { return moving; } set { moving = value; } }

	[SerializeField]
	[Range(1f, 100f)]
	private float steerSpeed;

	public float SteerSpeed
	{
		get
		{
			if (steerDirection == Vector2.left)
			{
				return steerSpeed;
			}
			else if (steerDirection == Vector2.right)
			{
				return -steerSpeed;
			}

			return 0f;
		}
	}

	[SerializeField]
	private Vector2 steerDirection;

	public Vector2 SteerDirection { get { return steerDirection; } }

	private Action<Vector2> steering = delegate { };

	public Action<Vector2> Steering { get { return steering; } set { steering = value; } }

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		rigidbody.gravityScale = 0f;
		rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
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

	public void Steer(Vector2 steerDirection)
	{
		this.steerDirection = steerDirection;
		Steering(steerDirection);
	}

	private void FixedUpdate()
	{
		rigidbody.AddForce(Velocity);

		var currentSpeed = rigidbody.velocity.magnitude;
		if (currentSpeed > speed)
		{
			rigidbody.AddForce(rigidbody.velocity.normalized * (speed - currentSpeed), ForceMode2D.Impulse);
		}

		rigidbody.MoveRotation(rigidbody.rotation + SteerSpeed * Time.fixedDeltaTime * 10f);
	}
}