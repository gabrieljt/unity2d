using System;
using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(CharacterInputDequeuer),
	typeof(CharacterMovement)
)]
public class Character : AActor
{
	[SerializeField]
	private int steps = 0;

	public int Steps { get { return steps; } }

	[SerializeField]
	private SpriteRenderer renderer;

	[SerializeField]
	private CharacterInputDequeuer inputDequeuer;

	[SerializeField]
	private CharacterMovement movement;

	public Action StepTaken = delegate { };

	private void Awake()
	{
		renderer = GetComponent<SpriteRenderer>();

		inputDequeuer = GetComponent<CharacterInputDequeuer>();
		inputDequeuer.InputsDequeued += OnInputsDequeued;

		movement = GetComponent<CharacterMovement>();
		movement.Moving += OnMoving;
	}

	private void OnInputsDequeued(Vector2 direction)
	{
		movement.Move(direction.normalized);
	}

	private void OnMoving(Vector2 direction)
	{
		renderer.flipX = direction.normalized.x > 0f;
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		var otherMovement = other.gameObject.GetComponent<CharacterMovement>();

		if (otherMovement)
		{
			renderer.flipX = (otherMovement.Position - movement.Position).normalized.x > 0f;
		}
	}

	public override void Enable()
	{
		base.Enable();
		gameObject.SetActive(true);
	}

	public override void Disable()
	{
		base.Disable();
		gameObject.SetActive(false);
	}

	public override void Dispose()
	{
		inputDequeuer.InputsDequeued -= OnInputsDequeued;
		movement.Moving -= OnMoving;
	}
}