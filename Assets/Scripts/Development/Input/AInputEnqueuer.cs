using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Input
{
	public abstract class AInputEnqueuer : MonoBehaviour, IDisposable
	{
		protected Queue<KeyCode> inputs = new Queue<KeyCode>();

		public Queue<KeyCode> Inputs { get { return inputs; } }

		public bool HasInputs { get { return inputs.Count > 0; } }

		[SerializeField]
		[Range(0, 10)]
		protected int maximumInputsPerFrame = 1;

		[SerializeField]
		[Range(0.1f, 5f)]
		protected float unlockInputsDelay = 0.5f;

		public Action<AInputEnqueuer> InputsEnqueued = delegate { };

		protected abstract void EnqueueInputs();

		private void Update()
		{
			EnqueueInputs();
			if (HasInputs)
			{
				InputsEnqueued(this);
			}
		}

		protected void LockInputs()
		{
			maximumInputsPerFrame = 0;
		}

		protected void UnlockInputs()
		{
			StartCoroutine(UnlockInputsCoroutine(unlockInputsDelay));
		}

		private IEnumerator UnlockInputsCoroutine(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
		}

		public abstract void Dispose();

		private void OnDestroy()
		{
			Dispose();
		}
	}
}