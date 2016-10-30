using System;
using UnityEngine;

public abstract class AActor : MonoBehaviour, IDestroyable, IDisposable
{
    public Action<AActor> Enabled = delegate { };

    public Action<AActor> Disabled = delegate { };

    private Action<IDestroyable> destroyed = delegate { };

    public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

    public virtual void Enable()
    {
        Enabled(this);
    }

    public virtual void Disable()
    {
        Disabled(this);
    }

    public abstract void Dispose();

    public void OnDestroy()
    {
        Destroyed(this);
        Dispose();
    }
}