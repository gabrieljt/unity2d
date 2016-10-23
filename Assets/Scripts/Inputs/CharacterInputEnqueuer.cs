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

	private List<Collision2D> collisions = new List<Collision2D>();

	[SerializeField]
	private Character character;

	protected override void Awake()
	{
		base.Awake();
		character = GetComponent<Character>();

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
			var inputsGenerated = Random.Range(1, 3);
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

		if (state == CharacterAIState.Moving)
		{
			foreach (var input in lastInputsReceived)
			{
				Enqueue(input);
			}
			Enqueue(KeyCode.None);
			return;
		}
	}

	private void OnInputsDequeued(Vector2 direction)
	{
		if (direction == Vector2.zero)
		{
			state = CharacterAIState.Idle;
			return;
		}

		if (state == CharacterAIState.Idle)
		{
			if (direction != Vector2.zero)
			{
				state = CharacterAIState.Moving;
				return;
			}
		}

		if (state == CharacterAIState.Moving)
		{
			var movement = character.GetComponent<CharacterMovement>();
			var point = movement.Position + direction * (character.GetComponent<CircleCollider2D>().radius + character.GetComponent<StepCounter>().StepSize);
			var something = Physics2D.OverlapPoint(point);

			if (something)
			{
			Debug.DrawLine(movement.Position, point, Color.cyan, 1f);
				lastInputsReceived.Clear();
				movement.Move(-direction);
				return;
			}
		}
	}

	public override void Dispose()
	{
		var dequeuerInstance = character.GetComponent<AInputDequeuer>();
		(dequeuerInstance as CharacterInputDequeuer).InputsDequeued -= OnInputsDequeued;
		base.Dispose();
	}
}