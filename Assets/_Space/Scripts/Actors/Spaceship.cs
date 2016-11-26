using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(SpaceshipInputDequeuer),
	typeof(SpaceshipMovement)
)]
[RequireComponent(
	typeof(StepCounter)
)]
public class Spaceship : AActor
{
	[SerializeField]
	private SpriteRenderer renderer;

	[SerializeField]
	private SpaceshipInputDequeuer inputDequeuer;

	[SerializeField]
	private SpaceshipMovement movement;

	private void Awake()
	{
		renderer = GetComponent<SpriteRenderer>();

		inputDequeuer = GetComponent<SpaceshipInputDequeuer>();
		inputDequeuer.InputsDequeued += OnInputsDequeued;

		movement = GetComponent<SpaceshipMovement>();
	}

	private void OnInputsDequeued(Vector2 direction)
	{
		movement.Move(direction);
	}

	public override void Dispose()
	{
		inputDequeuer.InputsDequeued -= OnInputsDequeued;
	}
}