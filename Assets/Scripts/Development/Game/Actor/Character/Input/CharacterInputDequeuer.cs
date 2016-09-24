using Game.Input;
using UnityEngine;

namespace Game.Actor
{
	[RequireComponent(
	typeof(Character)
	)]
	public class CharacterInputDequeuer : AInputDequeuer
	{
		[SerializeField]
		private Character character;

		public Character Character { get { return character; } }

		protected override void Awake()
		{
			character = GetComponent<Character>();

			character.MovementHalted += OnCharacterMovementHalt;
			character.Disabled += OnCharacterDisabled;
			character.Enabled += OnCharacterEnabled;

			base.Awake();
		}

		private void OnCharacterMovementHalt()
		{
			foreach (var inputEnqueuer in inputEnqueuers)
			{
				inputEnqueuer.Inputs.Clear();
			}
		}

		private void OnCharacterDisabled(AActor actor)
		{
			foreach (var inputEnqueuer in inputEnqueuers)
			{
				inputEnqueuer.LockInputs();
			}
		}

		private void OnCharacterEnabled(AActor actor)
		{
			foreach (var inputEnqueuer in inputEnqueuers)
			{
				inputEnqueuer.UnlockInputs();
			}
		}

		public override void OnInputsEnqueued(AInputEnqueuer inputQueue)
		{
			if (character.State == CharacterState.Idle)
			{
				if (inputQueue.HasInputs)
				{
					Vector2 direction = Vector2.zero;
					KeyCode input = inputQueue.Inputs.Dequeue();
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

				Debug.LogWarning(name + " InputEnqueuers: " + inputEnqueuers.Count);
			}
		}

		public override void Dispose()
		{
			character.MovementHalted -= OnCharacterMovementHalt;
			character.Disabled -= OnCharacterDisabled;
			character.Enabled -= OnCharacterEnabled;
		}
	}
}