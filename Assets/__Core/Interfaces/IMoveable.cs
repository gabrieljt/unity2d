using System;
using UnityEngine;

public interface IMoveable
{
    void Move(Vector2 direction);

    Vector2 Position { get; }

    Vector2 Direction { get; }

    Vector2 Velocity { get; }

    Action<Vector2> Moving { get; set; }
}