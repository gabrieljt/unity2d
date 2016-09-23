using System;
using System.Collections;
using System.Collections.Generic;
using TiledLevel;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour, IDisposable
{
	public enum GameStateType
	{
		Idle,
		LoadingLevel,
		InGame,
		Ended,
	}

	[SerializeField]
	private Camera camera;

	[SerializeField]
	private Map levelInstance;

	[SerializeField]
	[Range(1, 100)]
	private int level = 1;

	[SerializeField]
	private Text dungeonLevelLabel, stepsLeftLabel, stepsTakenLabel;

	[Serializable]
	public class Level
	{
		[SerializeField]
		[Range(1, 100)]
		private int id = 1;

		public int Id { get { return id; } }

		[SerializeField]
		private int stepsTaken = 0;

		public int StepsTaken { get { return stepsTaken; } }

		[SerializeField]
		private static int totalStepsTaken = 0;

		public static int TotalStepsTaken { get { return totalStepsTaken; } }

		[SerializeField]
		private int maximumSteps;

		public int MaximumSteps { get { return maximumSteps; } }

		public int StepsLeft { get { return maximumSteps - stepsTaken; } }

		public Level(int id)
		{
			this.id = id;
		}

		public void StepTaken()
		{
			++stepsTaken;
			++totalStepsTaken;
		}

		public void SetSize(out int width, out int height)
		{
			height = width = id + 9;
		}

		public void SetMaximumSteps(int level, MapDungeon.Room[] rooms, Vector2 tileMapOrigin)
		{
			maximumSteps = stepsTaken = 0;
			foreach (MapDungeon.Room room in rooms)
			{
				maximumSteps += (int)Vector2.Distance(room.Center, tileMapOrigin);
			}

			maximumSteps = Mathf.Clamp(maximumSteps / level * rooms.Length, level + rooms.Length, maximumSteps + level + rooms.Length);
		}
	}

	[SerializeField]
	private GameStateType state = GameStateType.Idle;

	private Stack<Level> levels = new Stack<Level>();

	[SerializeField]
	private Level currentLevel;

	private void Awake()
	{
		camera = FindObjectOfType<Camera>();
		Debug.Assert(camera);

		levelInstance = FindObjectOfType<Map>();
		Debug.Assert(levelInstance);

		levelInstance.Built += OnTileMapBuilt;
		levelInstance.GetComponent<MapDungeon>().Built += OnDungeonMapBuilt;
		levelInstance.GetComponent<MapDungeonSpawner>().Built += OnDungeonMapSpawnerBuilt;

		Debug.Assert(dungeonLevelLabel);
		Debug.Assert(stepsLeftLabel);
		Debug.Assert(stepsTakenLabel);
	}

	private void Start()
	{
		BuildLevel();
		UpdateUI();
	}

	private void Update()
	{
		if (state == GameStateType.InGame)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ResetLevel();
				return;
			}

			if (currentLevel.StepsLeft == 0)
			{
				ResetLevel();
			}
			UpdateUI();
		}
	}

	private void LateUpdate()
	{
		if (state == GameStateType.InGame)
		{
			var playerCharacter = FindObjectOfType<Character>();
			if (playerCharacter)
			{
				SetCameraPosition(playerCharacter.transform.position);
			}
		}
	}

	private void UpdateUI()
	{
		SetDungeonLevelLabel(currentLevel.Id);
		SetStepsLeftLabel(currentLevel.StepsLeft);
		SetStepsTakenLabel(Level.TotalStepsTaken);
	}

	private void SetCameraPosition(Vector3 position)
	{
		camera.transform.position = Vector3.back * 10f + position;
	}

	private void SetDungeonLevelLabel(int level)
	{
		dungeonLevelLabel.text = "Dungeon Level: " + level;
	}

	private void SetStepsLeftLabel(int stepsLeft)
	{
		stepsLeftLabel.text = "Steps Left: " + stepsLeft;
	}

	private void SetStepsTakenLabel(int steps)
	{
		stepsTakenLabel.text = "Steps Taken: " + steps;
	}

	private void BuildLevel()
	{
		state = GameStateType.LoadingLevel;
		if (level > levels.Count)
		{
			levels.Push(currentLevel = new Level(level));
		}

		int width, height;
		currentLevel.SetSize(out width, out height);
		camera.orthographicSize = Mathf.Sqrt(Mathf.Max(width, height));
		StartCoroutine(BuildNewTileMap(width, height));
	}

	private IEnumerator BuildNewTileMap(int width, int height)
	{
		yield return 0;

		levelInstance.Build(width, height, out levelInstance);
	}

	#region Callbacks

	private void OnStepTaken()
	{
		currentLevel.StepTaken();
	}

	private void OnExitReached()
	{
		state = GameStateType.Ended;
		UnregisterActorEvents();
		++level;
		BuildLevel();
	}

	public void OnTileMapBuilt()
	{
	}

	public void OnDungeonMapBuilt()
	{
		StartCoroutine(PopulateTileMap());
	}

	private void OnDungeonMapSpawnerBuilt()
	{
		foreach (var actorSpawner in levelInstance.GetComponent<MapDungeonSpawner>().GetComponents<ActorSpawner>())
		{
			actorSpawner.Spawned += OnActorSpawned;
		}
	}

	private void OnActorSpawned(ActorSpawner actorSpawner, GameObject spawnedActor)
	{
		actorSpawner.Spawned -= OnActorSpawned;
	}

	#endregion Callbacks

	private IEnumerator PopulateTileMap()
	{
		yield return 0;

		var playerCharacter = FindObjectOfType<Character>();
		var exit = FindObjectOfType<Exit>();

		bool populated = playerCharacter && exit;
		if (!populated)
		{
			StartCoroutine(PopulateTileMap());
		}
		else
		{
			StartCoroutine(StartNewLevel());
		}
	}

	private IEnumerator StartNewLevel()
	{
		yield return 0;
		state = GameStateType.InGame;
		RegisterActorEvents();
		currentLevel.SetMaximumSteps(level, levelInstance.GetComponent<MapDungeon>().Rooms, levelInstance.WorldPosition);
	}

	private void RegisterActorEvents()
	{
		var playerCharacter = FindObjectOfType<Character>();
		if (playerCharacter)
		{
			playerCharacter.StepTaken += OnStepTaken;
		}

		var exit = FindObjectOfType<Exit>();
		if (exit)
		{
			exit.Reached += OnExitReached;
		}
	}

	private void UnregisterActorEvents()
	{
		var playerCharacter = FindObjectOfType<Character>();
		if (playerCharacter)
		{
			playerCharacter.StepTaken -= OnStepTaken;
		}

		var exit = FindObjectOfType<Exit>();
		if (exit)
		{
			exit.Reached -= OnExitReached;
		}
	}

	private void ResetLevel()
	{
		UnregisterActorEvents();
		levels.Pop();
		BuildLevel();
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		levelInstance.Built -= OnTileMapBuilt;
		levelInstance.GetComponent<MapDungeon>().Built -= OnDungeonMapBuilt;
		levelInstance.GetComponent<MapDungeonSpawner>().Built -= OnDungeonMapSpawnerBuilt;
		FindObjectOfType<Character>().StepTaken -= OnStepTaken;
		FindObjectOfType<Exit>().Reached -= OnExitReached;
	}
}