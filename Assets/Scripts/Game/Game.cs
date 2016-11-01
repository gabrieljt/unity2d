using System;
using System.Collections;
using UnityEngine;

public enum GameState
{
    Unloaded,
    Loading,
    Started,
    Ended,
}

public class Game : MonoBehaviour, IDisposable, IDestroyable
{
    [SerializeField]
    private GameParams @params = new GameParams(1);

    public GameParams Params { get { return @params; } }

    [SerializeField]
    private GameState state = GameState.Unloaded;

    [SerializeField]
    private GameObject levelPrefab;

    [SerializeField]
    private Level level;

    public Level Level { get { return level; } }

    [SerializeField]
    private PlayerInputEnqueuer playerInputEnqueuerInstance;

    public static Game Instance
    {
        get
        {
            return FindObjectOfType<Game>();
        }
    }

    public Action Loading = delegate { };

    public Action Started = delegate { };

    internal Action Updated = delegate { };

    private Action<IDestroyable> destroyed = delegate { };

    public Action<IDestroyable> Destroyed { get { return destroyed; } set { destroyed = value; } }

    private void Awake()
    {
        gameObject.isStatic = true;
        Debug.Assert(levelPrefab);

        playerInputEnqueuerInstance = PlayerInputEnqueuer.Instance;
    }

    #region Load Level

    private void Start()
    {
        LoadLevel();
    }

    public void LoadLevel()
    {
        Debug.Assert(state == GameState.Unloaded);
        if (state == GameState.Unloaded)
        {
            StartCoroutine(LoadLevelCoroutine());
        }
    }

    private IEnumerator LoadLevelCoroutine()
    {
        yield return 0;
        state = GameState.Loading;
        @params = new GameParams(@params.Level);

        playerInputEnqueuerInstance.Inputs.Clear();
        playerInputEnqueuerInstance.LockInputs();

        level = (Instantiate(levelPrefab, transform, true) as GameObject).GetComponent<Level>();
        level.GetComponent<MapActorSpawners>().Built += OnActorSpawnersBuilt;
        level.Built += OnLevelBuilt;
        level.@params = @params.levelParams;

        Loading();
    }

    private void OnActorSpawnersBuilt(Type type)
    {
        level.GetComponent<MapActorSpawners>().Built -= OnActorSpawnersBuilt;
        var actorSpawners = level.GetComponents<ActorSpawner>();

        foreach (var actorSpawner in actorSpawners)
        {
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

    private void OnPlayerSpawned(ActorSpawner spawner, AActor actor)
    {
        spawner.Performed -= OnPlayerSpawned;
        var player = actor as Character;
        player.GetComponent<StepCounter>().StepTaken += OnStepTaken;
        player.Destroyed += OnPlayerDestroyed;

        playerInputEnqueuerInstance.Add(player);
    }

    private void OnPlayerDestroyed(IDestroyable destroyedComponent)
    {
        var player = destroyedComponent as Character;
        player.GetComponent<StepCounter>().StepTaken -= OnStepTaken;
        player.Destroyed -= OnPlayerDestroyed;
    }

    private void OnExitSpawned(ActorSpawner spawner, AActor actor)
    {
        spawner.Performed -= OnExitSpawned;
        var exit = actor as Exit;
        exit.Reached += OnExitReached;
        exit.Destroyed += OnExitDestroyed;
    }

    private void OnExitDestroyed(IDestroyable destroyedComponent)
    {
        var exit = destroyedComponent as Exit;

        exit.Reached -= OnExitReached;
        exit.Destroyed -= OnExitDestroyed;
    }

    #endregion Load Level

    #region Start Level

    private void OnLevelBuilt(Type type)
    {
        StartLevel();
    }

    private void StartLevel()
    {
        level.Built -= OnLevelBuilt;

        @params.levelParams = level.@params;

        StartCoroutine(StartLevelCoroutine());
    }

    private IEnumerator StartLevelCoroutine()
    {
        yield return 0;

        state = GameState.Started;

        @params.SetMaximumSteps(level.GetComponent<Map>(), level.GetComponent<MapDungeon>(), level.GetComponent<MapActorSpawners>());

        playerInputEnqueuerInstance.UnlockInputs();

        Started();
    }

    #endregion Start Level

    #region Update

    private void Update()
    {
        if (state == GameState.Started)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetLevel();
                return;
            }
        }

        Updated();
    }

    #endregion Update

    #region Logic

    private void OnStepTaken()
    {
        @params.StepTaken();

        if (@params.StepsLeft == 0)
        {
            ResetLevel();
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

    #endregion Logic

    #region End Level

    public void ResetLevel()
    {
        Debug.Assert(state == GameState.Started);
        if (state == GameState.Started)
        {
            ReloadLevel();
        }
    }

    private void ReloadLevel()
    {
        state = GameState.Unloaded;
        Destroy(level.gameObject);
        LoadLevel();
    }

    #endregion End Level

    public void Dispose()
    {
        state = GameState.Unloaded;
        StopAllCoroutines();
    }

    public void OnDestroy()
    {
        Destroyed(this);
        Dispose();
    }
}