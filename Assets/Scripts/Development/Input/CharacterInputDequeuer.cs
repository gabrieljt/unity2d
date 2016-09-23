using UnityEngine;

namespace Input
{
	[RequireComponent(
	typeof(Character)
	)]
	public class CharacterInputDequeuer : MonoBehaviour
	{
		[SerializeField]
		private Character character;

		public Character Character { get { return character; } }

		private void Awake()
		{
			character = GetComponent<Character>();
		}

		public void OnInputsEnqueued(AInputEnqueuer inputQueue)
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
			}
		}
	}
}