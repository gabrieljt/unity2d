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

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		rigidbody.gravityScale = 0f;
		rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

		speed = Random.Range(1f, speed);

		var targetDirection = (FindObjectOfType<Spaceship>().transform.position - transform.position).normalized;
		direction = (Quaternion.AngleAxis(Random.Range(-60f, 60f), Vector3.up) * targetDirection).normalized;

		Debug.DrawRay(transform.position, targetDirection, Color.green, 1f);
		Debug.DrawRay(transform.position, direction, Color.red, 1f);
	}

	private void FixedUpdate()
	{
		rigidbody.MovePosition(Position + Velocity * Time.fixedDeltaTime);
		rigidbody.MoveRotation(rigidbody.rotation + speed * Time.fixedDeltaTime * 10f);
	}
}