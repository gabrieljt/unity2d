using Game.Actor;
using Game.Input;
using Game.Level.Tiled;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
	public class MapDungeonGame : MonoBehaviour, IDisposable
	{
		public enum GameStateType
		{
			Loading,
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
		private GameStateType state = GameStateType.Loading;

		private Stack<Level> levels = new Stack<Level>();

		[SerializeField]
		private Level currentLevel;

		private Character player;

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
				SetCameraPosition(player.transform.position);
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
			StartCoroutine(StartLevelCoroutine());
		}

		private IEnumerator StartLevelCoroutine()
		{
			state = GameStateType.Loading;
			camera.gameObject.SetActive(false);
			yield return 0;
			PlayerInputEnqueuer.Instance.LockInputs();

			if (level > levels.Count)
			{
				levels.Push(currentLevel = new Level(level));
			}

			levelInstance.GetComponent<MapDungeonActorSpawner>().Built += OnMapDungeonActorSpawnerBuilt;
			levelInstance.Built += OnMapDungeonLevelBuilt;

			levelInstance.Load(new MapDungeonLevelParams(currentLevel.Id));

			StartCoroutine(BuildLevelCoroutine());
		}

		private void OnMapDungeonActorSpawnerBuilt(Type mapDungeonActorSpawnerType)
		{
			levelInstance.GetComponent<MapDungeonActorSpawner>().Built -= OnMapDungeonActorSpawnerBuilt;
			var actorSpawners = levelInstance.GetComponents<ActorSpawner>();

			foreach (var actorSpawner in actorSpawners)
			{
				actorSpawner.Spawned += OnActorSpawnerSpawned;
				if (actorSpawner.actorType == ActorType.Player)
				{
					actorSpawner.Spawned += OnPlayerSpawned;
				}

				if (actorSpawner.actorType == ActorType.Exit)
				{
					actorSpawner.Spawned += OnExitSpawned;
				}
			}
		}

		private void OnExitSpawned(ActorSpawner actorSpawner, AActor actor)
		{
			actorSpawner.Spawned -= OnExitSpawned;
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

		private void OnPlayerSpawned(ActorSpawner actorSpawner, AActor actor)
		{
			actorSpawner.Spawned -= OnPlayerSpawned;
			player = actor as Character;
			player.StepTaken += OnStepTaken;
			player.Destroyed += OnPlayerDestroyed;

			var playerInputDequeuer = player.GetComponent<CharacterInputDequeuer>() as AInputDequeuer;
			PlayerInputEnqueuer.Add(ref playerInputDequeuer);
		}

		private void OnPlayerDestroyed(MonoBehaviour player)
		{
			if (this.player == player)
			{
				this.player.StepTaken -= OnStepTaken;
				this.player.Destroyed -= OnPlayerDestroyed;
				this.player = null;
			}
		}

		private void OnActorSpawnerSpawned(ActorSpawner actorSpawner, AActor actor)
		{
			actorSpawner.Spawned -= OnActorSpawnerSpawned;
			actorSpawner.enabled = false;
		}

		private IEnumerator BuildLevelCoroutine()
		{
			yield return 0;
			levelInstance.Build();
		}

		private void ResetLevel()
		{
			levels.Pop();
			ReloadLevel();
		}

		private void ReloadLevel()
		{
			Dispose();
			LoadLevel();
		}

		private void OnStepTaken()
		{
			currentLevel.StepTaken();
		}

		private void OnExitReached(Character character)
		{
			if (character.gameObject.CompareTag(ActorType.Player.ToString()))
			{
				state = GameStateType.Ended;
				++level;
				ReloadLevel();
			}
		}

		private void OnMapDungeonLevelBuilt(Type levelComponentBuiltType)
		{
			levelInstance.Built -= OnMapDungeonLevelBuilt;

			StartCoroutine(StartNewLevel());
		}

		private IEnumerator StartNewLevel()
		{
			yield return 0;
			state = GameStateType.InGame;

			var mapCenter = levelInstance.GetComponent<Map>().Center;
			camera.orthographicSize = Mathf.Min(mapCenter.x, mapCenter.y);
			SetCameraPosition(player.transform.position);
			camera.gameObject.SetActive(true);

			currentLevel.SetMaximumSteps(level, levelInstance.GetComponent<MapDungeon>().Rooms, mapCenter);

			PlayerInputEnqueuer.Instance.UnlockInputs();
		}

		private void OnDestroy()
		{
			Dispose();
		}

		public void Dispose()
		{
			StopAllCoroutines();
			levelInstance.Dispose();
		}
	}
}