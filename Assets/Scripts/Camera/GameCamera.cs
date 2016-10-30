using System;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private Action<IDestroyable> destroyed = delegate { };

    public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

    private void Awake()
    {
        var gameInstance = Game.Instance;
        gameInstance.Loading += OnGameLoading;
        gameInstance.Started += OnGameStarted;
        gameInstance.Destroyed += OnGameDestroyed;
    }

    private void OnGameLoading()
    {
        GetComponent<Camera>().enabled = false;
    }

    private void OnGameStarted()
    {
        var mapCenter = Game.Instance.Level.GetComponent<Map>().Center;
        var camera = GetComponent<Camera>();

        camera.orthographicSize = Mathf.Min(mapCenter.x, mapCenter.y);
        camera.transform.position = Vector3.back * 10f + (Vector3)mapCenter;
        camera.enabled = true;
    }

    private void OnGameDestroyed(IDestroyable destroyedComponent)
    {
        var gameInstance = destroyedComponent as Game;
        gameInstance.Loading -= OnGameLoading;
        gameInstance.Started -= OnGameStarted;
        gameInstance.Destroyed -= OnGameDestroyed;
    }
}