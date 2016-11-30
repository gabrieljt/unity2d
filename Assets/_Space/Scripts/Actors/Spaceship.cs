using System;
using UnityEngine;

[RequireComponent(
	typeof(SpriteRenderer),
	typeof(SpaceshipInputDequeuer),
	typeof(SpaceshipMovement)
)]
[RequireComponent(
	typeof(SpaceshipLaserGun)
)]
public class Spaceship : MonoBehaviour, IDestroyable, IDisposable
{
	[SerializeField]
	private SpaceshipInputDequeuer inputDequeuer;

	[SerializeField]
	private SpaceshipMovement movement;

	[SerializeField]
	private SpaceshipLaserGun laserGun;

	private Action<IDestroyable> destroyed = delegate { };

	public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

	private void Awake()
	{
		inputDequeuer = GetComponent<SpaceshipInputDequeuer>();
		inputDequeuer.InputsDequeued += OnInputsDequeued;

		movement = GetComponent<SpaceshipMovement>();

		laserGun = GetComponentInChildren<SpaceshipLaserGun>();
	}

	private void OnInputsDequeued(Vector2 direction, Vector2 steerDirection, bool fire)
	{
		movement.Move(direction);
		movement.Steer(steerDirection);

		if (fire)
		{
			laserGun.Fire();
		}
	}

	public void OnDestroy()
	{
		Destroyed(this);
		Dispose();
	}

	public void Dispose()
	{
		inputDequeuer.InputsDequeued -= OnInputsDequeued;
	}
}