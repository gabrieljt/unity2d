using UnityEngine;

[RequireComponent(
	typeof(Character)
)]
public class CharacterInputDequeuer : AInputDequeuer
{
	[SerializeField]
	private Character character;

	private void Awake()
	{
		character = GetComponent<Character>();

		character.MovementHalted += OnCharacterMovementHalted;
	}

	private void OnCharacterMovementHalted()
	{
		foreach (var enqueuer in enqueuers)
		{
			enqueuer.Inputs.Clear();
		}
	}

	public override void OnInputsEnqueued(AInputEnqueuer enqueuer)
	{
		if (character.State == CharacterState.Idle)
		{
			if (enqueuer.HasInputs)
			{
				Vector2 direction = Vector2.zero;
				KeyCode input = enqueuer.Inputs.Dequeue();
				switch (input)
				{
					case KeyCode.UpArrow:
						direction = Vector2.up;
						break;

					case KeyCode.DownArrow:
						direction = Vector2.down;
						break;

					case KeyCode.LeftArrow:
						direction = Vector2.left;
						break;

					case KeyCode.RightArrow:
						direction = Vector2.right;
						break;
				}

				character.SetDestination(direction);
			}

			Debug.LogWarning(GetType() + " dequeueing from " + enqueuer.GetType() + " | Total InputEnqueuers: " + enqueuers.Count);
		}
	}

	public override void Dispose()
	{
		character.MovementHalted -= OnCharacterMovementHalted;
	}
}