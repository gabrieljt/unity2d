using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AInputEnqueuer : MonoBehaviour, IDestroyable, IDisposable
{
	protected Queue<KeyCode> inputs = new Queue<KeyCode>();

	protected HashSet<AInputDequeuer> dequeuers = new HashSet<AInputDequeuer>();

	public HashSet<AInputDequeuer> Dequeuers { get { return dequeuers; } }

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

	public void Add(ref AInputEnqueuer instance, ref AInputDequeuer dequeuer)
	{
		Debug.Assert(!instance.Dequeuers.Contains(dequeuer));
		Debug.Assert(!dequeuer.Enqueuers.Contains(instance));

		if (instance.Dequeuers.Contains(dequeuer))
		{
			return;
		}

		RegisterDequeuer(ref instance, ref dequeuer);

		dequeuer.Enqueuers.Add(instance);
		instance.Dequeuers.Add(dequeuer);

		Debug.Assert(dequeuer.Enqueuers.Count <= 2);
	}

	private void RegisterDequeuer(ref AInputEnqueuer instance, ref AInputDequeuer dequeuer)
	{
		instance.InputsEnqueued += dequeuer.OnInputsEnqueued;
		(dequeuer as IDestroyable).Destroyed += OnDequeuerDestroyed;

		var otherEnqueuer = dequeuer.GetComponent<AInputEnqueuer>();
		if (otherEnqueuer && otherEnqueuer != instance)
		{
			otherEnqueuer.enabled = false;
		}
	}

	public void Remove(ref AInputEnqueuer instance, ref AInputDequeuer dequeuer)
	{
		Debug.Assert(instance.Dequeuers.Contains(dequeuer));
		Debug.Assert(dequeuer.Enqueuers.Contains(instance));

		if (!instance.Dequeuers.Contains(dequeuer))
		{
			return;
		}

		UnregisterDequeuer(ref instance, ref dequeuer);

		dequeuer.Enqueuers.Remove(instance);
		instance.Dequeuers.Remove(dequeuer);

		Debug.Assert(dequeuer.Enqueuers.Count >= 0);
	}

	private void UnregisterDequeuer(ref AInputEnqueuer instance, ref AInputDequeuer dequeuer)
	{
		instance.InputsEnqueued -= dequeuer.OnInputsEnqueued;
		(dequeuer as IDestroyable).Destroyed -= OnDequeuerDestroyed;

		var otherEnqueuer = dequeuer.GetComponent<AInputEnqueuer>();
		if (otherEnqueuer && otherEnqueuer != instance)
		{
			otherEnqueuer.enabled = true;
		}
	}

	protected abstract void OnDequeuerDestroyed(MonoBehaviour dequeuerBehaviour);

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