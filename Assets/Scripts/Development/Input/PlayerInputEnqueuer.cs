using System;
using UnityEngine;

namespace Input
{
	public class PlayerInputEnqueuer : AInputEnqueuer
	{
		[SerializeField]
		private AInputDequeuer inputDequeuer;

		private void Awake()
		{
			gameObject.isStatic = true;
		}

		protected override void EnqueueInputs()
		{
			if (UnityEngine.Input.anyKey && inputs.Count < maximumInputsPerFrame)
			{
				if (UnityEngine.Input.GetKey(KeyCode.UpArrow))
				{
					inputs.Enqueue(KeyCode.UpArrow);
					return;
				}

				if (UnityEngine.Input.GetKey(KeyCode.DownArrow))
				{
					inputs.Enqueue(KeyCode.DownArrow);
					return;
				}

				if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
				{
					inputs.Enqueue(KeyCode.LeftArrow);
					return;
				}

				if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
				{
					inputs.Enqueue(KeyCode.RightArrow);
					return;
				}
			}
		}

		
	}
}