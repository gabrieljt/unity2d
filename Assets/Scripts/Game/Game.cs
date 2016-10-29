using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(Game))]
public class GameInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (Application.isPlaying)
        {
            LoadLevelButton();
            ResetLevelButton();
            DisposeButton();
        }
    }

    private void LoadLevelButton()
    {
        if (GUILayout.Button("Load Level"))
        {
            var game = (Game)target;
            game.LoadLevel();
        }
    }

    private void ResetLevelButton()
    {
        if (GUILayout.Button("Reset Level"))
        {
            var game = (Game)target;
            game.ResetLevel();
        }
    }

    private void DisposeButton()
    {
        if (GUILayout.Button("Dispose"))
        {
            var game = (Game)target;
            game.Dispose();
        }
    }
}

#endif

public enum GameState
{
    Unloaded,
    Loading,
    InGame,
    Ended,
}

public class Game : MonoBehaviour, IDisposable
{
    [SerializeField]
    private GameParams @params = new GameParams(1);

    public GameParams Params { get { return @params; } }

    [SerializeField]
    private GameState state = GameState.Unloaded;

    public GameState State { get { return state; } }

    [SerializeField]
    private Level level;

    [SerializeField]
    private Camera camera;

    private Character player;

    private Exit exit;

    public static Game Instance
    {
        get
        {
            return FindObjectOfType<Game>();
        }
    }

    public Action LevelStarted = delegate { };

    public Action LevelReloaded = delegate { };

    private void Awake()
    {
        gameObject.isStatic = true;

        camera = FindObjectOfType<Camera>();
        Debug.Assert(camera);

        level = Level.Instance;
        Debug.Assert(level);
    }

    #region Start

    private void Start()
    {
        LoadLevel();
    }

    public void LoadLevel()
    {
        Debug.Assert(state == GameState.Unloaded);
        if (state == GameState.Unloaded)
        {
            StartCoroutine(StartLevelCoroutine());
        }
    }

    private IEnumerator StartLevelCoroutine()
    {
        yield return 0;
        state = GameState.Loading;
        @params = new GameParams(@params.Level);
        camera.enabled = false;

        var playerInputEnqueuerInstance = PlayerInputEnqueuer.Instance;
        playerInputEnqueuerInstance.Inputs.Clear();
        playerInputEnqueuerInstance.LockInputs();

        level.GetComponent<MapActorSpawners>().Built += OnActorSpawnersBuilt;
        level.Built += OnLevelBuilt;

        level.Load(@params.levelParams);

        StartCoroutine(BuildLevelCoroutine());
    }

    private IEnumerator BuildLevelCoroutine()
    {
        yield return 0;
        level.Build();
    }

    private void OnActorSpawnersBuilt(Type type)
    {
        level.GetComponent<MapActorSpawners>().Built -= OnActorSpawnersBuilt;
        var actorSpawners = level.GetComponents<ActorSpawner>();

        foreach (var actorSpawner in actorSpawners)
        {
            actorSpawner.Performed += OnActorSpawnerPerformed;
            if (actorSpawner.type == ActorType.Player)
            {
                actorSpawner.Performed += OnPlayerSpawned;
            }

            if (actorSpawner.type == ActorType.Exit)
            {
                actorSpawner.Performed += OnExitSpawned;
            }
        }
    }

    private void OnActorSpawnerPerformed(ActorSpawner spawner, AActor actor)
    {
        spawner.Performed -= OnActorSpawnerPerformed;
        spawner.enabled = false;
    }

    private void OnPlayerSpawned(ActorSpawner spawner, AActor actor)
    {
        spawner.Performed -= OnPlayerSpawned;
        player = actor as Character;
        player.GetComponent<StepCounter>().StepTaken += OnStepTaken;
        player.Destroyed += OnPlayerDestroyed;

        var inputDequeuer = player.GetComponent<CharacterInputDequeuer>() as AInputDequeuer;
        PlayerInputEnqueuer.Add(player, ref inputDequeuer);
    }

    private void OnPlayerDestroyed(MonoBehaviour player)
    {
        if (this.player == player)
        {
            this.player.GetComponent<StepCounter>().StepTaken -= OnStepTaken;
            this.player.Destroyed -= OnPlayerDestroyed;
            this.player = null;
        }
    }

    private void OnStepTaken()
    {
        @params.StepTaken();
    }

    private void OnExitSpawned(ActorSpawner spawner, AActor actor)
    {
        spawner.Performed -= OnExitSpawned;
        exit = actor as Exit;
        exit.Reached += OnExitReached;
        exit.Destroyed += OnExitDestroyed;
    }

    private void OnExitDestroyed(MonoBehaviour exit)
    {
        if (this.exit == exit)
        {
            this.exit.Reached -= OnExitReached;
            this.exit.Destroyed -= OnExitDestroyed;
            this.exit = null;
        }
    }

    private void OnExitReached(Character character)
    {
        if (character.gameObject.CompareTag(ActorType.Player.ToString()))
        {
            state = GameState.Ended;
            @params = new GameParams(@params.Level + 1);
            ReloadLevel();
        }
    }

    private void OnLevelBuilt(Type type)
    {
        level.Built -= OnLevelBuilt;

        @params.levelParams = level.Params;

        StartCoroutine(StartLevel());
    }

    private IEnumerator StartLevel()
    {
        yield return 0;
        state = GameState.InGame;

        var mapCenter = level.GetComponent<Map>().Center;
        camera.orthographicSize = Mathf.Min(mapCenter.x, mapCenter.y);
        SetCameraPosition(mapCenter);
        camera.enabled = true;

        @params.SetMaximumSteps(level.GetComponent<Map>(), level.GetComponent<MapDungeon>(), level.GetComponent<MapActorSpawners>());

        PlayerInputEnqueuer.Instance.UnlockInputs();

        LevelStarted();
    }

    #endregion Start

    #region Update

    private void Update()
    {
        if (state == GameState.InGame)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetLevel();
                return;
            }

            if (@params.StepsLeft == 0)
            {
                ResetLevel();
            }
        }
    }

    public void ResetLevel()
    {
        Debug.Assert(state == GameState.InGame);
        if (state == GameState.InGame)
        {
            ReloadLevel();
        }
    }

    private void ReloadLevel()
    {
        Dispose();
        LoadLevel();
        LevelReloaded();
    }

    #endregion Update

    private void SetCameraPosition(Vector3 position)
    {
        camera.transform.position = Vector3.back * 10f + position;
    }

    public void Dispose()
    {
        state = GameState.Unloaded;
        StopAllCoroutines();
        level.Dispose(true);
    }
}