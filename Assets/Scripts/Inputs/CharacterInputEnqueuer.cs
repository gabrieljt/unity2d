using System.Collections.Generic;
using UnityEngine;

public enum CharacterAIState
{
	Idle,
	Moving,
}

[RequireComponent(
	typeof(Character)
)]
public class CharacterInputEnqueuer : AInputEnqueuer
{
	[SerializeField]
	private CharacterAIState state;

	private List<KeyCode> lastInputsReceived = new List<KeyCode>();

	private HashSet<Collider2D> otherColliders = new HashSet<Collider2D>();

	private Vector2 inputDirection;

	[SerializeField]
	private Character character;

	private CharacterMovement movement;

	private CircleCollider2D collider;

	private Vector2 EscapeDirection
	{
		get
		{
			var newDirection = Vector2.zero;
			foreach (var otherCollider in otherColliders)
			{
				newDirection += movement.Position - (new Vector2(otherCollider.transform.position.x, otherCollider.transform.position.y) + otherCollider.offset);
			}

			return newDirection.normalized;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		character = GetComponent<Character>();
		movement = character.GetComponent<CharacterMovement>();
		collider = character.GetComponent<CircleCollider2D>();

		var instance = this as AInputEnqueuer;
		var dequeuerInstance = character.GetComponent<AInputDequeuer>();
		Add(ref instance, ref dequeuerInstance);
	}

	protected override void EnqueueInputs()
	{
		if (state == CharacterAIState.Idle)
		{
			lastInputsReceived.Clear();
			var inputsGenerated = Random.Range(0, maximumInputsPerUpdate);
			if (inputsGenerated > 0)
			{
				for (int i = 0; i < inputsGenerated; i++)
				{
					var generatedInput = Random.Range(0, 4);
					var input = KeyCode.None;
					if (generatedInput == 0)
					{
						input = KeyCode.UpArrow;
					}

					if (generatedInput == 1)
					{
						input = KeyCode.DownArrow;
					}

					if (generatedInput == 2)
					{
						input = KeyCode.LeftArrow;
					}

					if (generatedInput == 3)
					{
						input = KeyCode.RightArrow;
					}
					lastInputsReceived.Add(input);
					Enqueue(input);
				}
				return;
			}
			Enqueue(KeyCode.None);
			return;
		}

		if (state == CharacterAIState.Moving)
		{
			foreach (var input in lastInputsReceived)
			{
				Enqueue(input);
			}
			if (lastInputsReceived.Count == 0)
			{
				Enqueue(KeyCode.None);
			}
			return;
		}
	}

	protected override void OnInputsDequeued(Vector2 direction)
	{
		if (state == CharacterAIState.Idle)
		{
			if (direction != Vector2.zero)
			{
				inputDirection = direction;
				state = CharacterAIState.Moving;
				return;
			}
		}

		if (state == CharacterAIState.Moving)
		{
			if (inputDirection == Vector2.zero)
			{
				state = CharacterAIState.Idle;
				return;
			}
		}

		movement.Move(inputDirection);
		DrawDebugRays(direction, ref inputDirection);
		return;
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		lastInputsReceived.Clear();
		otherColliders.Add(other.collider);
		inputDirection = EscapeDirection;
	}

	private void OnCollisionStay2D()
	{
		inputDirection = EscapeDirection;
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		otherColliders.Remove(other.collider);
		inputDirection = EscapeDirection;

		if (inputDirection == Vector2.zero)
		{
			TryToEscape(other);
			return;
		}
	}

	private void TryToEscape(Collision2D other)
	{
		var direction = (movement.Position - (new Vector2(other.transform.position.x, other.transform.position.y) + other.collider.offset)).normalized;
		var point = movement.Position + direction;
		var something = Physics2D.OverlapPoint(point);
		if (!something)
		{
			Debug.DrawRay(movement.Position, direction, Color.white, 1f);
			inputDirection = direction;
			return;
		}
		Debug.DrawRay(movement.Position, direction, Color.blue, 1f);
		return;
	}

	private void DrawDebugRays(Vector2 direction, ref Vector2 inputDirection)
	{
		if (inputDirection != direction)
		{
			Debug.DrawRay(movement.Position, inputDirection.normalized * collider.radius, Color.cyan);
			return;
		}
		Debug.DrawRay(movement.Position, direction.normalized * collider.radius, Color.magenta);
		return;
	}
}