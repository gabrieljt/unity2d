using System;
using UnityEngine;

public interface IDestroyable
{
    void OnDestroy();

    Action<MonoBehaviour> Destroyed { get; set; }
}