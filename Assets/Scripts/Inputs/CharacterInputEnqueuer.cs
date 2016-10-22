using UnityEngine;

[RequireComponent(
	typeof(Character),
	typeof(CharacterInputDequeuer)
)]
public class CharacterInputEnqueuer : AInputEnqueuer
{
	[SerializeField]
	private Character character;

	[SerializeField]
	private CharacterInputDequeuer dequeuer;

	protected override void Awake()
	{
		base.Awake();
		character = GetComponent<Character>();
		dequeuer = GetComponent<CharacterInputDequeuer>();

		var instance = this as AInputEnqueuer;
		var dequeuerInstance = dequeuer as AInputDequeuer;
		Add(ref instance, ref dequeuerInstance);
	}

	// TODO: character input logic (AI)
	protected override void EnqueueInputs()
	{
		var inputsToGenerate = Random.Range(0, maximumInputsPerUpdate - inputs.Count);

		for (int i = 0; i < inputsToGenerate; i++)
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

			Enqueue(input);
		}
	}
}