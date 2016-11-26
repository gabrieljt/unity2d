using System;
using UnityEngine;

public abstract class AActor : MonoBehaviour, IDestroyable, IDisposable
{
    private Action<IDestroyable> destroyed = delegate { };

    public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

    public abstract void Dispose();

    public void OnDestroy()
    {
        Destroyed(this);
        Dispose();
    }
}