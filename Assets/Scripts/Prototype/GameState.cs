using System;
using System.Collections;
using System.Collections.Generic;
using TiledLevel;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour, IDisposable
{
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
		}

		public void SetSize(out int width, out int height)
		{
			height = width = id + 9;
		}

		public void SetMaximumSteps(int level, MapDungeon.Room[] dungeons, Vector2 tileMapOrigin)
		{
			maximumSteps = stepsTaken = 0;
			foreach (MapDungeon.Room dungeon in dungeons)
			{
				maximumSteps += (int)Vector2.Distance(dungeon.Center, tileMapOrigin);
			}

			maximumSteps = Mathf.Clamp(maximumSteps / level * dungeons.Length, level + dungeons.Length, maximumSteps + level + dungeons.Length);
		}
	}

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

	private void LateUpdate()
	{
		var playerCharacter = FindObjectOfType<Character>();
		if (playerCharacter)
		{
			SetCameraPosition(playerCharacter.transform.position);
		}
	}

	private void UpdateUI()
	{
		SetDungeonLevelLabel(currentLevel.Id);
		SetStepsLeftLabel(currentLevel.StepsLeft);
		var playerCharacter = FindObjectOfType<Character>();
		if (playerCharacter)
		{
			SetStepsTakenLabel(playerCharacter.Steps);
		}
		else
		{
			SetStepsTakenLabel(0);
		}
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
		var levelParams = new LevelParams();
		levelParams.Width = width;
		levelParams.Height = height;
		IMapParams mapParams = levelParams;
		levelInstance.Build(ref mapParams);
	}

	#region Callbacks

	private void OnStepTaken()
	{
		currentLevel.StepTaken();
	}

	private void OnExitReached()
	{
		++level;
		BuildLevel();
	}

	public void OnTileMapBuilt(IMapParams mapParams)
	{
	}

	public void OnDungeonMapBuilt(IMapDungeonParams dungeonMapParams)
	{
		StartCoroutine(PopulateTileMap(dungeonMapParams));
	}

	private void OnDungeonMapSpawnerBuilt(IMapDungeonSpawnerParams dungeonMapSpawnerParams)
	{
		foreach (var actorSpawner in levelInstance.GetComponent<MapDungeonSpawner>().GetComponents<ActorSpawner>())
		{
			actorSpawner.Spawned += OnActorSpawned;
		}
	}

	private void OnActorSpawned(ActorSpawner actorSpawner, GameObject spawnedActor)
	{
		if (actorSpawner.IsType<Character>())
		{
			var playerCharacter = spawnedActor.GetComponent<Character>();
			playerCharacter.StepTaken += OnStepTaken;
		}
		else
		{
			if (actorSpawner.IsType<Exit>())
			{
				var exit = spawnedActor.GetComponent<Exit>();
				exit.Reached += OnExitReached;
			}
		}

		actorSpawner.Spawned -= OnActorSpawned;
	}

	#endregion Callbacks

	private IEnumerator PopulateTileMap(IMapDungeonParams dungeonMapParams)
	{
		bool populated = FindObjectOfType<Character>() && FindObjectOfType<Exit>();
		yield return 0;

		if (!populated)
		{
			StartCoroutine(PopulateTileMap(dungeonMapParams));
		}
		else
		{
			currentLevel.SetMaximumSteps(level, dungeonMapParams.Dungeons, levelInstance.WorldPosition);
		}
	}

	private void ResetLevel()
	{
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
		FindObjectOfType<Character>().StepTaken -= OnStepTaken;
		FindObjectOfType<Exit>().Reached -= OnExitReached;
	}
}