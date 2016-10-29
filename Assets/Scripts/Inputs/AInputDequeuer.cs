﻿using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AInputDequeuer : MonoBehaviour, IDestroyable
{
    protected HashSet<AInputEnqueuer> enqueuers = new HashSet<AInputEnqueuer>();

    public HashSet<AInputEnqueuer> Enqueuers { get { return enqueuers; } }

    public Action<Vector2> InputsDequeued = delegate { };

    private Action<MonoBehaviour> destroyed = delegate { };

    public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

    public abstract void OnInputsEnqueued(AInputEnqueuer inputQueue);

    public void OnDestroy()
    {
        Destroyed(this);
    }
}