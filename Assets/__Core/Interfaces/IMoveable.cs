using System;
using UnityEngine;

public interface IMoveable
{
	float Speed { get; }

	Vector2 Position { get; }

	Vector2 Direction { get; }

	Vector2 Velocity { get; }

	void Move(Vector2 direction);

	Action<Vector2> Moving { get; set; }
}