using System;
using UnityEngine;

[RequireComponent(
	typeof(Rigidbody2D)
)]
public class MeteorMovement : MonoBehaviour, IMoveable
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

	public Vector2 Velocity { get { return direction * speed; } }

	private Action<Vector2> moving = delegate { };

	public Action<Vector2> Moving { get { return moving; } set { moving = value; } }

	public float Mass { get { return rigidbody.mass; } }

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		rigidbody.gravityScale = 0f;
		rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
		rigidbody.mass = UnityEngine.Random.Range(1f, rigidbody.mass);

		speed = UnityEngine.Random.Range(rigidbody.mass, speed * rigidbody.mass);

		var targetDirection = (FindObjectOfType<Spaceship>().transform.position - transform.position).normalized;
		direction = (Quaternion.AngleAxis(UnityEngine.Random.Range(-60f, 60f), Vector3.up) * targetDirection).normalized;

		Debug.DrawRay(transform.position, targetDirection, Color.green, 1f);
		Debug.DrawRay(transform.position, direction, Color.red, 1f);
	}

	private void FixedUpdate()
	{
		Move(Position + Velocity * Time.fixedDeltaTime);
	}

	public void Move(Vector2 position)
	{
		rigidbody.MovePosition(position);
		rigidbody.MoveRotation(rigidbody.rotation + speed * Time.fixedDeltaTime * 10f);
	}
}