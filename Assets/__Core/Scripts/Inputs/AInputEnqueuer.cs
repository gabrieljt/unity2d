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
	protected int maximumInputsPerUpdate = 1;

	private int MaximumInputsPerUpdate { get; set; }

	[SerializeField]
	[Range(0.1f, 5f)]
	protected float unlockInputsDelay = 0.5f;

	public Action<AInputEnqueuer> InputsEnqueued = delegate { };

	private Action<IDestroyable> destroyed = delegate { };

	public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

	protected virtual void Awake()
	{
		MaximumInputsPerUpdate = maximumInputsPerUpdate;
	}

	public void Add(ref AInputDequeuer dequeuer)
	{
		Debug.Assert(!Dequeuers.Contains(dequeuer));
		Debug.Assert(!dequeuer.Enqueuers.Contains(this));

		if (Dequeuers.Contains(dequeuer))
		{
			return;
		}

		RegisterDequeuer(ref dequeuer);

		dequeuer.Enqueuers.Add(this);
		Dequeuers.Add(dequeuer);

		Debug.Assert(dequeuer.Enqueuers.Count <= 2);
	}

	private void RegisterDequeuer(ref AInputDequeuer dequeuer)
	{
		InputsEnqueued += dequeuer.OnInputsEnqueued;
		(dequeuer as IDestroyable).Destroyed += OnDequeuerDestroyed;

		var otherEnqueuer = dequeuer.GetComponent<AInputEnqueuer>();
		if (otherEnqueuer && otherEnqueuer != this)
		{
			otherEnqueuer.enabled = false;
		}
	}

	public void Remove(ref AInputDequeuer dequeuer)
	{
		Debug.Assert(Dequeuers.Contains(dequeuer));
		Debug.Assert(dequeuer.Enqueuers.Contains(this));

		if (!Dequeuers.Contains(dequeuer))
		{
			return;
		}

		UnregisterDequeuer(ref dequeuer);

		dequeuer.Enqueuers.Remove(this);
		Dequeuers.Remove(dequeuer);

		Debug.Assert(dequeuer.Enqueuers.Count >= 0);
	}

	private void UnregisterDequeuer(ref AInputDequeuer dequeuer)
	{
		InputsEnqueued -= dequeuer.OnInputsEnqueued;
		(dequeuer as IDestroyable).Destroyed -= OnDequeuerDestroyed;

		var otherEnqueuer = dequeuer.GetComponent<AInputEnqueuer>();
		if (otherEnqueuer && otherEnqueuer != this)
		{
			otherEnqueuer.enabled = true;
		}
	}

	protected virtual void OnDequeuerDestroyed(IDestroyable destroyedComponent)
	{
		var dequeuer = destroyedComponent as AInputDequeuer;
		Remove(ref dequeuer);
	}

	protected abstract void EnqueueInputs();

	protected void Enqueue(KeyCode input)
	{
		if (inputs.Count < maximumInputsPerUpdate)
		{
			inputs.Enqueue(input);
		}
	}

	private void FixedUpdate()
	{
		EnqueueInputs();
		if (HasInputs)
		{
			InputsEnqueued(this);
		}
	}

	public void LockInputs()
	{
		maximumInputsPerUpdate = 0;
	}

	public void UnlockInputs()
	{
		StartCoroutine(UnlockInputsCoroutine(unlockInputsDelay));
	}

	private IEnumerator UnlockInputsCoroutine(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		maximumInputsPerUpdate = MaximumInputsPerUpdate;
	}

	public void OnDestroy()
	{
		Destroyed(this);
		Dispose();
	}

	public virtual void Dispose()
	{
		var dequeuersList = new List<AInputDequeuer>(dequeuers);
		foreach (var dequeuer in dequeuersList)
		{
			var dequeuerInstance = dequeuer;
			Remove(ref dequeuerInstance);
		}

		dequeuers.Clear();
	}
}