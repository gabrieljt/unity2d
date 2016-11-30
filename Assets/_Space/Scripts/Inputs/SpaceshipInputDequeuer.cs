using System;
using UnityEngine;

public class SpaceshipInputDequeuer : AInputDequeuer
{
	public Action<Vector2, Vector2, bool> InputsDequeued = delegate { };

	public override void OnInputsEnqueued(AInputEnqueuer enqueuer)
	{
		var direction = Vector2.zero;
		var steerDirection = Vector2.zero;
		var fire = false;
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
					steerDirection += Vector2.left;
					break;

				case KeyCode.RightArrow:
					steerDirection += Vector2.right;
					break;

				case KeyCode.Space:
					fire = true;
					break;
			}
		}

		InputsDequeued(direction.normalized, steerDirection.normalized, fire);
	}
}