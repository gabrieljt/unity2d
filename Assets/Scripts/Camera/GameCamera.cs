using System;
using UnityEngine;

public class GameCamera : MonoBehaviour, IDestroyable, IDisposable
{
    private Action<MonoBehaviour> destroyed = delegate { };

    public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

    private void Awake()
    {
        var gameInstance = Game.Instance;
        gameInstance.LevelStarted += OnGameLevelStarted;
        gameInstance.LevelReloaded += OnGameLevelReloaded;
    }

    private void OnGameLevelStarted()
    {
        var mapCenter = Level.Instance.GetComponent<Map>().Center;
        var camera = GetComponent<Camera>();

        camera.orthographicSize = Mathf.Min(mapCenter.x, mapCenter.y);
        camera.transform.position = Vector3.back * 10f + (Vector3)mapCenter;
        camera.enabled = true;
    }

    private void OnGameLevelReloaded()
    {
        GetComponent<Camera>().enabled = false;
    }

    public void Dispose()
    {
        var gameInstance = Game.Instance;
        if (gameInstance)
        {
            gameInstance.LevelStarted -= OnGameLevelStarted;
            gameInstance.LevelReloaded -= OnGameLevelReloaded;
        }
    }

    public void OnDestroy()
    {
        Dispose();
    }
}