using System;
using UnityEngine;

[RequireComponent(
	typeof(Camera)
)]
public class GameCamera : MonoBehaviour, IDestroyable, IDisposable
{
	[SerializeField]
	private Game gameInstance;

	[SerializeField]
	private Camera camera;

	private Action<IDestroyable> destroyed = delegate { };

	public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

	private void Awake()
	{
		gameInstance = Game.Instance;
		gameInstance.Loading += OnGameLoading;
		gameInstance.Started += OnGameStarted;
		gameInstance.Destroyed += OnGameDestroyed;

		camera = GetComponent<Camera>();
	}

	private void OnGameLoading()
	{
		camera.enabled = false;
	}

	private void OnGameStarted()
	{
		var mapCenter = gameInstance.Level.GetComponent<Map>().Center;

		camera.orthographicSize = Mathf.Min(mapCenter.x, mapCenter.y);
		camera.transform.position = Vector3.back + (Vector3)mapCenter;
		camera.enabled = true;
	}

	private void OnGameDestroyed(IDestroyable destroyedComponent)
	{
		gameInstance.Loading -= OnGameLoading;
		gameInstance.Started -= OnGameStarted;
		gameInstance.Destroyed -= OnGameDestroyed;
	}

	public void Dispose()
	{
		if (gameInstance)
		{
			gameInstance.Loading -= OnGameLoading;
			gameInstance.Started -= OnGameStarted;
			gameInstance.Destroyed -= OnGameDestroyed;
		}
	}

	public void OnDestroy()
	{
		Destroyed(this);
		Dispose();
	}
}