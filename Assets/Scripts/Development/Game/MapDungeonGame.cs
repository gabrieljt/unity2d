using Game.Actor;
using Game.Input;
using Game.Level.Tiled;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
	public enum GameState
	{
		Unloaded,
		Loading,
		InGame,
		Ended,
	}

	public class MapDungeonGame : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private GameState state = GameState.Unloaded;

		[SerializeField]
		private MapDungeonGameParams mapDungeonGameParams = new MapDungeonGameParams(1);

		[SerializeField]
		private MapDungeonLevel mapDungeonLevel;

		[SerializeField]
		private Camera camera;

		private Character player;

		private Exit exit;

		[SerializeField]
		private Text dungeonLevelLabel, stepsLeftLabel, stepsTakenLabel;

		private void Awake()
		{
			gameObject.isStatic = true;

			camera = FindObjectOfType<Camera>();
			Debug.Assert(camera);

			mapDungeonLevel = FindObjectOfType<MapDungeonLevel>();
			Debug.Assert(mapDungeonLevel);

			Debug.Assert(dungeonLevelLabel);
			Debug.Assert(stepsLeftLabel);
			Debug.Assert(stepsTakenLabel);
		}

		#region Start

		private void Start()
		{
			LoadLevel();
		}

		private void LoadLevel()
		{
			StartCoroutine(StartLevelCoroutine());
		}

		private IEnumerator StartLevelCoroutine()
		{
			state = GameState.Loading;
			camera.gameObject.SetActive(false);
			yield return 0;

			PlayerInputEnqueuer.Instance.LockInputs();

			mapDungeonLevel.GetComponent<MapDungeonActorSpawner>().Built += OnMapDungeonActorSpawnerBuilt;
			mapDungeonLevel.Built += OnMapDungeonLevelBuilt;

			mapDungeonLevel.Load(mapDungeonGameParams.MapDungeonLevelParams);

			StartCoroutine(BuildLevelCoroutine());
		}

		private IEnumerator BuildLevelCoroutine()
		{
			yield return 0;
			mapDungeonLevel.Build();
		}

		private void OnMapDungeonActorSpawnerBuilt(Type mapDungeonActorSpawnerType)
		{
			mapDungeonLevel.GetComponent<MapDungeonActorSpawner>().Built -= OnMapDungeonActorSpawnerBuilt;
			var actorSpawners = mapDungeonLevel.GetComponents<ActorSpawner>();

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

		private void OnActorSpawnerSpawned(ActorSpawner actorSpawner, AActor actor)
		{
			actorSpawner.Spawned -= OnActorSpawnerSpawned;
			actorSpawner.enabled = false;
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

		private void OnStepTaken()
		{
			mapDungeonGameParams.StepTaken();
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

		private void OnExitReached(Character character)
		{
			if (character.gameObject.CompareTag(ActorType.Player.ToString()))
			{
				state = GameState.Ended;
				mapDungeonGameParams = new MapDungeonGameParams(mapDungeonGameParams.Level + 1);
				ReloadLevel();
			}
		}

		private void OnMapDungeonLevelBuilt(Type levelComponentBuiltType)
		{
			mapDungeonLevel.Built -= OnMapDungeonLevelBuilt;

			StartCoroutine(StartLevel());
		}

		private IEnumerator StartLevel()
		{
			yield return 0;
			state = GameState.InGame;

			UpdateUI();
			var mapCenter = mapDungeonLevel.GetComponent<Map>().Center;
			camera.orthographicSize = Mathf.Min(mapCenter.x, mapCenter.y);
			SetCameraPosition(player.transform.position);
			camera.gameObject.SetActive(true);

			mapDungeonGameParams.SetMaximumSteps(mapDungeonLevel.GetComponent<MapDungeon>(), mapDungeonLevel.GetComponent<MapDungeonActorSpawner>(), mapCenter);

			PlayerInputEnqueuer.Instance.UnlockInputs();
		}

		#endregion Start

		#region Update

		private void Update()
		{
			if (state == GameState.InGame)
			{
				if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
				{
					ResetLevel();
					return;
				}

				if (mapDungeonGameParams.StepsLeft == 0)
				{
					ResetLevel();
				}
				UpdateUI();
			}
		}

		private void ResetLevel()
		{
			ReloadLevel();
		}

		private void ReloadLevel()
		{
			Dispose();
			LoadLevel();
		}

		private void UpdateUI()
		{
			SetDungeonLevelLabel(mapDungeonGameParams.Level);
			SetStepsLeftLabel(mapDungeonGameParams.StepsLeft);
			SetStepsTakenLabel(MapDungeonGameParams.TotalStepsTaken);
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

		#endregion Update

		#region LateUpdate

		private void LateUpdate()
		{
			if (state == GameState.InGame)
			{
				SetCameraPosition(player.transform.position);
			}
		}

		private void SetCameraPosition(Vector3 position)
		{
			camera.transform.position = Vector3.back * 10f + position;
		}

		#endregion LateUpdate

		public void Dispose()
		{
			StopAllCoroutines();
			mapDungeonLevel.Dispose();
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}