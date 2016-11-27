﻿using System;
using UnityEngine;

public class SpaceshipInputDequeuer : AInputDequeuer
{
	public Action<Vector2, Vector2> InputsDequeued = delegate { };

	public override void OnInputsEnqueued(AInputEnqueuer enqueuer)
	{
		var direction = Vector2.zero;
		var steering = Vector2.zero;
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
					steering += Vector2.left;
					break;

				case KeyCode.RightArrow:
					steering += Vector2.right;
					break;
			}
		}

		InputsDequeued(direction.normalized, steering.normalized);
	}
}