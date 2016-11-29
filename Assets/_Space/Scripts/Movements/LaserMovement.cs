using UnityEngine;

[RequireComponent(
	typeof(Laser),
	typeof(Rigidbody2D)
)]
public class LaserMovement : MonoBehaviour, IMoveable
{
	[SerializeField]
	private Laser laser;

	[SerializeField]
	private Rigidbody2D rigidbody;

	[SerializeField]
	[Range(1f, 100f)]
	private float speed;

	public float Speed { get { return speed; } }

	public Vector2 Direction { get { return transform.up; } }

	public Vector2 Position { get { return rigidbody.position; } }

	public Vector2 Velocity { get { return Direction * speed; } }

	private void Awake()
	{
		laser = GetComponent<Laser>();

		rigidbody = GetComponent<Rigidbody2D>();
		rigidbody.gravityScale = 0f;
		rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
		rigidbody.interpolation = RigidbodyInterpolation2D.None;
		rigidbody.isKinematic = true;
	}

	private void FixedUpdate()
	{
		rigidbody.MovePosition(Position + Velocity * Time.fixedDeltaTime);
	}
}