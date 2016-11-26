using System;
using UnityEngine;

public interface IMoveable
{
	float Speed { get; }

	Vector2 Position { get; }

	Vector2 Direction { get; }

	Vector2 Velocity { get; }
}

public interface IWalkable : IMoveable
{
	void Move(Vector2 direction);

	Action<Vector2> Moving { get; set; }
}

public interface IRideable : IMoveable
{
	void Move(Vector2 direction, Vector2 steering);

	float SteeringSpeed { get; }

	Vector2 Steering { get; }

	Action<Vector2, Vector2> Moving { get; set; }
}