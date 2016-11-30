using System;
using UnityEngine;

[RequireComponent(
	typeof(Rigidbody2D)
)]
public class MeteorMovement : MonoBehaviour, IMoveable, ISteerable
{
	#region Unity Components

	[SerializeField]
	private Rigidbody2D rigidbody;

	public float Mass { get { return rigidbody.mass; } }

	#endregion Unity Components

	#region IMoveable

	[SerializeField]
	[Range(1f, 100f)]
	private float speed;

	public float Speed { get { return speed; } }

	[SerializeField]
	private Vector2 direction;

	public Vector2 Direction { get { return direction; } }

	public Vector2 Position { get { return rigidbody.position; } }

	public Vector2 Velocity { get { return direction * speed; } }

	private Action<Vector2> moving = delegate { };

	public Action<Vector2> Moving { get { return moving; } set { moving = value; } }

	#endregion IMoveable

	#region ISteerable

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

	#endregion ISteerable

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		rigidbody.gravityScale = 0f;
		rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

		rigidbody.mass = UnityEngine.Random.Range(1f, rigidbody.mass);
		speed = UnityEngine.Random.Range(Mathf.Min(rigidbody.mass, speed), rigidbody.mass * speed);
		steerSpeed = UnityEngine.Random.Range(Mathf.Min(rigidbody.mass, steerSpeed), rigidbody.mass * steerSpeed);
		steerDirection = UnityEngine.Random.Range(0, 2) % 2 == 0 ? Vector2.left : Vector2.right;

		var targetDirection = (FindObjectOfType<Spaceship>().transform.position - transform.position).normalized;
		direction = (Quaternion.AngleAxis(UnityEngine.Random.Range(-60f, 60f), Vector2.up) * targetDirection).normalized;

#if UNITY_EDITOR

		Debug.DrawRay(transform.position, targetDirection, Color.green, 1f);
		Debug.DrawRay(transform.position, direction, Color.red, 1f);

#endif
	}

	private void Start()
	{
		Move(Velocity);
	}

	public void Move(Vector2 velocity)
	{
		rigidbody.velocity = velocity;
	}

	public void Steer(Vector2 steerDirection)
	{
		rigidbody.MoveRotation(rigidbody.rotation + steerSpeed * Time.fixedDeltaTime * 10f);
	}

	private void FixedUpdate()
	{
		Steer(steerDirection);
	}
}