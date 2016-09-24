using Game.Input;
using UnityEngine;

namespace Game.Actor
{
	[RequireComponent(
	typeof(Character),
	typeof(CharacterInputDequeuer)
)]
	public class CharacterInputEnqueuer : AInputEnqueuer
	{
		[SerializeField]
		private Character character;

		[SerializeField]
		private CharacterInputDequeuer characterInputDequeuer;

		private void Awake()
		{
			character = GetComponent<Character>();
			characterInputDequeuer = GetComponent<CharacterInputDequeuer>();

			var instance = this as AInputEnqueuer;
			var characterInputDequeuerInstance = characterInputDequeuer as AInputDequeuer;
			Add(ref instance, ref characterInputDequeuerInstance);
		}

		// TODO: character input logic (AI)
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
			var instance = this as AInputEnqueuer;
			var characterInputDequeuerInstance = characterInputDequeuer as AInputDequeuer;
			Remove(ref instance, ref characterInputDequeuerInstance);
		}

		protected override void OnInputDequeuerDestroyed(MonoBehaviour obj)
		{
		}
	}
}