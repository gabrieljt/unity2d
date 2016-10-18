using UnityEngine;

[RequireComponent(
	typeof(Character),
	typeof(CharacterMovement)
)]
public class CharacterInputDequeuer : AInputDequeuer
{
	[SerializeField]
	private CharacterMovement characterMovement;

	private void Awake()
	{
		characterMovement = GetComponent<CharacterMovement>();
		characterMovement.MovementStopped += OnCharacterMovementStopped;
	}

	private void OnCharacterMovementStopped()
	{
		foreach (var enqueuer in enqueuers)
		{
			enqueuer.Inputs.Clear();
		}
	}

	public override void OnInputsEnqueued(AInputEnqueuer enqueuer)
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

			characterMovement.Move(direction);
			//Debug.LogWarning(GetType() + " dequeueing from " + enqueuer.GetType() + " | Total InputEnqueuers: " + enqueuers.Count);
		}
	}

	public override void Dispose()
	{
		characterMovement.MovementStopped -= OnCharacterMovementStopped;
	}
}