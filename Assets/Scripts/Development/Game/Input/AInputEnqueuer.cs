using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Input
{
	public abstract class AInputEnqueuer : MonoBehaviour, IDestroyable, IDisposable
	{
		protected Queue<KeyCode> inputs = new Queue<KeyCode>();

		protected HashSet<AInputDequeuer> inputDequeuers = new HashSet<AInputDequeuer>();

		public HashSet<AInputDequeuer> InputDequeuers { get { return inputDequeuers; } }

		public Queue<KeyCode> Inputs { get { return inputs; } }

		public bool HasInputs { get { return inputs.Count > 0; } }

		[SerializeField]
		[Range(0, 10)]
		protected int maximumInputsPerFrame = 1;

		private int MaximumInputsPerFrame { get; set; }

		[SerializeField]
		[Range(0.1f, 5f)]
		protected float unlockInputsDelay = 0.5f;

		public Action<AInputEnqueuer> InputsEnqueued = delegate { };

		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		protected virtual void Awake()
		{
			MaximumInputsPerFrame = maximumInputsPerFrame;
		}

		public void Add(ref AInputEnqueuer instance, ref AInputDequeuer inputDequeuer)
		{
			Debug.Assert(!instance.InputDequeuers.Contains(inputDequeuer));
			Debug.Assert(!inputDequeuer.InputEnqueuers.Contains(instance));

			if (instance.InputDequeuers.Contains(inputDequeuer))
			{
				return;
			}

			RegisterInputDequeuer(ref instance, ref inputDequeuer);

			inputDequeuer.InputEnqueuers.Add(instance);
			instance.InputDequeuers.Add(inputDequeuer);

			Debug.Assert(inputDequeuer.InputEnqueuers.Count <= 2);
		}

		private void RegisterInputDequeuer(ref AInputEnqueuer instance, ref AInputDequeuer inputDequeuer)
		{
			instance.InputsEnqueued += inputDequeuer.OnInputsEnqueued;
			(inputDequeuer as IDestroyable).Destroyed += OnInputDequeuerDestroyed;

			var otherEnqueuer = inputDequeuer.GetComponent<AInputEnqueuer>();
			if (otherEnqueuer && otherEnqueuer != instance)
			{
				otherEnqueuer.enabled = false;
			}
		}

		public void Remove(ref AInputEnqueuer instance, ref AInputDequeuer inputDequeuer)
		{
			Debug.Assert(instance.InputDequeuers.Contains(inputDequeuer));
			Debug.Assert(inputDequeuer.InputEnqueuers.Contains(instance));

			if (!instance.InputDequeuers.Contains(inputDequeuer))
			{
				return;
			}

			UnregisterInputDequeuer(ref instance, ref inputDequeuer);

			inputDequeuer.InputEnqueuers.Remove(instance);
			instance.InputDequeuers.Remove(inputDequeuer);

			Debug.Assert(inputDequeuer.InputEnqueuers.Count >= 0);
		}

		private void UnregisterInputDequeuer(ref AInputEnqueuer instance, ref AInputDequeuer inputDequeuer)
		{
			instance.InputsEnqueued -= inputDequeuer.OnInputsEnqueued;
			(inputDequeuer as IDestroyable).Destroyed -= OnInputDequeuerDestroyed;

			var otherEnqueuer = inputDequeuer.GetComponent<AInputEnqueuer>();
			if (otherEnqueuer && otherEnqueuer != instance)
			{
				otherEnqueuer.enabled = true;
			}
		}

		protected abstract void OnInputDequeuerDestroyed(MonoBehaviour obj);

		protected abstract void EnqueueInputs();

		private void Update()
		{
			EnqueueInputs();
			if (HasInputs)
			{
				InputsEnqueued(this);
			}
		}

		public void LockInputs()
		{
			maximumInputsPerFrame = 0;
		}

		public void UnlockInputs()
		{
			StartCoroutine(UnlockInputsCoroutine(unlockInputsDelay));
		}

		private IEnumerator UnlockInputsCoroutine(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			maximumInputsPerFrame = MaximumInputsPerFrame;
		}

		public abstract void Dispose();

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}