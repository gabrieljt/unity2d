using Game.Actor;
using Game.Level.Tiled;
using System;
using System.Collections;
using System.Collections.Generic;
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
	private MapDungeonLevel levelInstance;

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

	private Character playerCharacter;

	private Exit exit;

	private void Awake()
	{
		camera = FindObjectOfType<Camera>();
		Debug.Assert(camera);

		levelInstance = FindObjectOfType<MapDungeonLevel>();
		Debug.Assert(levelInstance);

		Debug.Assert(dungeonLevelLabel);
		Debug.Assert(stepsLeftLabel);
		Debug.Assert(stepsTakenLabel);
	}

	private void Start()
	{
		LoadLevel();
		UpdateUI();
	}

	private void Update()
	{
		if (state == GameStateType.InGame)
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
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
			if (playerCharacter)
			{
				SetCameraPosition(playerCharacter.transform.position);
			}
		}
		else
		{
			SetCameraPosition(levelInstance.transform.position);
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

	private void LoadLevel()
	{
		state = GameStateType.LoadingLevel;
		if (level > levels.Count)
		{
			levels.Push(currentLevel = new Level(level));
		}

		levelInstance.Built += OnMapDungeonLevelBuilt;
		levelInstance.Load(currentLevel.Id);
		StartCoroutine(BuildLevelCoroutine());
	}

	private IEnumerator BuildLevelCoroutine()
	{
		yield return 0;
		{
			levelInstance.Build();
		}
	}

	private void ResetLevel()
	{
		levels.Pop();
		ReloadLevel();
	}

	private void ReloadLevel()
	{
		UnregisterActorEvents();
		DisableActors();
		levelInstance.Built -= OnMapDungeonLevelBuilt;
		levelInstance.Dispose();
		LoadLevel();
	}

	#region Callbacks

	private void OnStepTaken()
	{
		currentLevel.StepTaken();
	}

	private void OnExitReached()
	{
		state = GameStateType.Ended;
		++level;
		ReloadLevel();
	}

	private void OnMapDungeonLevelBuilt()
	{
		levelInstance.Built -= OnMapDungeonLevelBuilt;
		StartCoroutine(StartNewLevel());
	}

	#endregion Callbacks

	private IEnumerator StartNewLevel()
	{
		yield return 0;
		state = GameStateType.InGame;

		var spawnedActors = levelInstance.MapDungeonLevelBuilder.MapDungeonActorSpawner.spawnedActors;

		playerCharacter = spawnedActors[ActorType.Player][0] as Character;
		exit = spawnedActors[ActorType.Exit][0] as Exit;

		var mapCenter = levelInstance.MapDungeonLevelBuilder.Map.Center;
		camera.orthographicSize = Mathf.Min(mapCenter.x, mapCenter.y);
		currentLevel.SetMaximumSteps(level, levelInstance.GetComponent<MapDungeon>().Rooms, mapCenter);

		RegisterActorEvents();
		StartCoroutine(EnableActors(playerCharacter, exit));
	}

	private void RegisterActorEvents()
	{
		if (playerCharacter)
		{
			playerCharacter.StepTaken += OnStepTaken;
		}

		if (exit)
		{
			exit.Reached += OnExitReached;
		}
	}

	private void UnregisterActorEvents()
	{
		if (playerCharacter)
		{
			playerCharacter.StepTaken -= OnStepTaken;
		}

		if (exit)
		{
			exit.Reached -= OnExitReached;
		}
	}

	private IEnumerator EnableActors(Character playerCharacter, Exit exit)
	{
		yield return 0;

		playerCharacter.Enable();
		exit.Enable();
	}

	private void DisableActors()
	{
		playerCharacter.Disable();
		exit.Disable();
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		levelInstance.Built -= OnMapDungeonLevelBuilt;

		UnregisterActorEvents();
	}
}