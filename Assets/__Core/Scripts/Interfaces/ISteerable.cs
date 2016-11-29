using System;
using UnityEngine;

public interface ISteerable
{
	void Steer(Vector2 direction);

	float SteeringSpeed { get; }

	Vector2 SteerDirection { get; }

	Action<Vector2> Steering { get; set; }
}