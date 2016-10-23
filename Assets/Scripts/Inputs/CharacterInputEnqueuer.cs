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
		var movement = character.GetComponent<CharacterMovement>();
		var collider = character.GetComponent<CircleCollider2D>();
		var stepCounter = character.GetComponent<StepCounter>();

		if (direction == Vector2.zero)
		{
			state = CharacterAIState.Idle;
			return;
		}

		if (state == CharacterAIState.Idle)
		{
			Vector2 endPoint;
			Collider2D something;

			if (SomethingAhead(direction, movement, collider, stepCounter, out endPoint, out something))
			{
				movement.Move(Vector2.zero);
				return;
			}
			state = CharacterAIState.Moving;
		}

		if (state == CharacterAIState.Moving)
		{
			Vector2 endPoint;
			Collider2D something;

			if (SomethingAhead(direction, movement, collider, stepCounter, out endPoint, out something))
			{
				state = CharacterAIState.Idle;
				return;
			}
		}
	}

	private void OnCollisionEnter2D()
	{
		state = CharacterAIState.Idle;
	}

	private static bool SomethingAhead(Vector2 direction, CharacterMovement movement, CircleCollider2D collider, StepCounter stepCounter, out Vector2 endPoint, out Collider2D somethingAhead)
	{
		endPoint = movement.Position + direction * (stepCounter.StepSize * 0.75f);
		somethingAhead = Physics2D.OverlapCircle(endPoint, stepCounter.StepSize * 0.25f);

		if (somethingAhead)
		{
			Debug.Log(somethingAhead.transform.name);
			Debug.DrawLine(movement.Position, endPoint, Color.cyan, 1f);
		}
		else
		{
			Debug.DrawLine(movement.Position, endPoint, Color.magenta);
		}
		return somethingAhead;
	}

	public override void Dispose()
	{
		var dequeuerInstance = character.GetComponent<AInputDequeuer>();
		(dequeuerInstance as CharacterInputDequeuer).InputsDequeued -= OnInputsDequeued;
		base.Dispose();
	}
}