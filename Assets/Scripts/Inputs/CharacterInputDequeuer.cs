using System;
using UnityEngine;

public class CharacterInputDequeuer : AInputDequeuer
{
	public Action<Vector2> InputsDequeued = delegate { };

	public override void OnInputsEnqueued(AInputEnqueuer enqueuer)
	{
		var direction = Vector2.zero;
		while (enqueuer.HasInputs)
		{
			var input = enqueuer.Inputs.Dequeue();
			switch (input)
			{
				case KeyCode.UpArrow:
					direction += Vector2.up;
					break;

				case KeyCode.DownArrow:
					direction += Vector2.down;
					break;

				case KeyCode.LeftArrow:
					direction += Vector2.left;
					break;

				case KeyCode.RightArrow:
					direction += Vector2.right;
					break;
			}
		}

		InputsDequeued(direction);
	}
}