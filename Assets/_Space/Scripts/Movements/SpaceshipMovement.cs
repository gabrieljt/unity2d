using System;
using UnityEngine;

[RequireComponent(
	typeof(Rigidbody2D)
)]
public class SpaceshipMovement : MonoBehaviour, IRideable
{
	[SerializeField]
	[Range(1f, 100f)]
	private float speed;

	public float Speed { get { return speed; } }

	[SerializeField]
	[Range(1f, 100f)]
	private float steeringSpeed;

	public float SteeringSpeed
	{
		get
		{
			if (steering == Vector2.left)
			{
				return steeringSpeed;
			}
			else if (steering == Vector2.right)
			{
				return -steeringSpeed;
			}

			return 0f;
		}
	}

	[SerializeField]
	private Vector2 direction;

	public Vector2 Direction { get { return direction; } }

	[SerializeField]
	private Vector2 steering;

	public Vector2 Steering { get { return steering; } }

	[SerializeField]
	private Rigidbody2D rigidbody;

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

	private Action<Vector2, Vector2> moving = delegate { };

	public Action<Vector2, Vector2> Moving { get { return moving; } set { moving = value; } }

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

	public void Move(Vector2 direction, Vector2 steering)
	{
		this.direction = direction;
		this.steering = steering;
		Moving(direction, steering);
	}

	private void FixedUpdate()
	{
		rigidbody.AddForce(Velocity);

		var currentSpeed = rigidbody.velocity.magnitude;
		if (currentSpeed > speed)
		{
			rigidbody.AddForce(rigidbody.velocity.normalized * (speed - currentSpeed), ForceMode2D.Impulse);
		}

		rigidbody.MoveRotation(rigidbody.rotation + SteeringSpeed * Time.fixedDeltaTime * 10f);
	}
}