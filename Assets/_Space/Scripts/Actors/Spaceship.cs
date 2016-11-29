using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(SpaceshipInputDequeuer),
	typeof(SpaceshipMovement)
)]
[RequireComponent(
	typeof(SpaceshipGun)
)]
public class Spaceship : AActor
{
	[SerializeField]
	private SpriteRenderer renderer;

	[SerializeField]
	private SpaceshipInputDequeuer inputDequeuer;

	[SerializeField]
	private SpaceshipMovement movement;

	[SerializeField]
	private SpaceshipGun gun;

	private void Awake()
	{
		renderer = GetComponent<SpriteRenderer>();

		inputDequeuer = GetComponent<SpaceshipInputDequeuer>();
		inputDequeuer.InputsDequeued += OnInputsDequeued;

		movement = GetComponent<SpaceshipMovement>();

		gun = GetComponentInChildren<SpaceshipGun>();
	}

	private void OnInputsDequeued(Vector2 direction, Vector2 steering, bool fire)
	{
		movement.Move(direction);
		movement.Steer(steering);

		if (fire)
		{
			gun.Fire();
		}
	}

	public override void Dispose()
	{
		inputDequeuer.InputsDequeued -= OnInputsDequeued;
	}
}