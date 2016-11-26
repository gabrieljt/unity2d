using System;
using UnityEngine;

public abstract class ALevelComponent : MonoBehaviour, IBuildable, IDestroyable, IDisposable
{
	private Action<Type> built = delegate { };

	public Action<Type> Built { get { return built; } set { built = value; } }

	private Action<IDestroyable> destroyed = delegate { };

	public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

	public abstract void Build();

	public abstract void Dispose();

	public virtual void OnDestroy()
	{
		Destroyed(this);
		Dispose();
	}
}