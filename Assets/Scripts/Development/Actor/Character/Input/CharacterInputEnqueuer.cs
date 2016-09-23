using Actor;
using System;
using UnityEngine;

namespace Input
{
	[RequireComponent(
	typeof(Character),
	typeof(CharacterInputDequeuer)
)]
	public class CharacterInputEnqueuer : AInputEnqueuer, IDisposable
	{
		[SerializeField]
		private Character character;

		[SerializeField]
		private CharacterInputDequeuer characterInputDequeuer;

		private void Awake()
		{
			character = GetComponent<Character>();
			character.Enabled += OnCharacterEnabled;
			character.Disabled += OnCharacterDisabled;

			characterInputDequeuer = GetComponent<CharacterInputDequeuer>();
			InputsEnqueued += characterInputDequeuer.OnInputsEnqueued;
		}

		private void OnCharacterEnabled(AActor obj)
		{
			UnlockInputs();
		}

		private void OnCharacterDisabled(AActor obj)
		{
			LockInputs();
		}

		protected override void EnqueueInputs()
		{
			if (inputs.Count < 1)
			{
				int generatedInput = UnityEngine.Random.Range(0, 4);

				if (generatedInput == 0)
				{
					inputs.Enqueue(KeyCode.UpArrow);
					return;
				}

				if (generatedInput == 1)
				{
					inputs.Enqueue(KeyCode.DownArrow);
					return;
				}

				if (generatedInput == 2)
				{
					inputs.Enqueue(KeyCode.LeftArrow);
					return;
				}

				if (generatedInput == 3)
				{
					inputs.Enqueue(KeyCode.RightArrow);
					return;
				}
			}
		}

		public override void Dispose()
		{
			InputsEnqueued -= characterInputDequeuer.OnInputsEnqueued;
			character.Enabled -= OnCharacterEnabled;
			character.Disabled -= OnCharacterDisabled;
		}
	}
}