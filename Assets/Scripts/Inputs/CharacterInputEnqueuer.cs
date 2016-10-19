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
		if (inputs.Count < 1)
		{
			var generatedInput = Random.Range(0, 4);

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
		var dequeuerInstance = dequeuer as AInputDequeuer;
		Remove(ref instance, ref dequeuerInstance);
	}

	protected override void OnDequeuerDestroyed(MonoBehaviour obj)
	{
	}
}