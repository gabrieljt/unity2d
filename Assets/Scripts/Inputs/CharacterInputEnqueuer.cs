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

	[SerializeField]
	private Character character;

	private StepCounter stepCounter;
	private CharacterMovement movement;
	private CircleCollider2D collider;

	protected override void Awake()
	{
		base.Awake();
		character = GetComponent<Character>();

		movement = character.GetComponent<CharacterMovement>();
		collider = character.GetComponent<CircleCollider2D>();
		stepCounter = character.GetComponent<StepCounter>();

		var instance = this as AInputEnqueuer;
		var dequeuerInstance = character.GetComponent<AInputDequeuer>();
		(dequeuerInstance as CharacterInputDequeuer).InputsDequeued += OnInputsDequeued;
		Add(ref instance, ref dequeuerInstance);
	}

	// TODO: character input logic (AI)
	protected override void EnqueueInputs()
	{
		if (state == CharacterAIState.Idle)
		{
			lastInputsReceived.Clear();
			var inputsGenerated = Random.Range(0, 2);
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

	private void OnInputsDequeued(Vector2 direction)
	{
		if (state == CharacterAIState.Idle)
		{
			if (direction == Vector2.zero || lastInputsReceived.Count == 0)
			{
				state = CharacterAIState.Idle;
				return;
			}
			state = CharacterAIState.Moving;
			return;
		}

		if (state == CharacterAIState.Moving)
		{
			if (otherColliders.Count > 0)
			{
				lastInputsReceived.Clear();

				var newDirection = Vector2.zero;
				foreach (var other in otherColliders)
				{
					newDirection += (new Vector2(other.transform.position.x, other.transform.position.y) + other.offset) - movement.Position;
				}

				Debug.DrawRay(movement.Position, -newDirection.normalized * collider.radius, Color.cyan, 1f);
				movement.Move(-newDirection.normalized);
				return;
			}

			if (direction == Vector2.zero || lastInputsReceived.Count == 0)
			{
				state = CharacterAIState.Idle;
				return;
			}

			Debug.DrawRay(movement.Position, direction * collider.radius, Color.magenta);
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		otherColliders.Add(other.collider);
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		otherColliders.Remove(other.collider);
	}

	public override void Dispose()
	{
		var dequeuerInstance = character.GetComponent<AInputDequeuer>();
		(dequeuerInstance as CharacterInputDequeuer).InputsDequeued -= OnInputsDequeued;
		base.Dispose();
	}
}