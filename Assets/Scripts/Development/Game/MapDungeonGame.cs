using Game.Actor;
using Game.Input;
using Game.Level.Tiled;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeonGame))]
	public class MapDungeonGameInspector : Editor
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
				var game = (MapDungeonGame)target;
				game.LoadLevel();
			}
		}

		private void ResetLevelButton()
		{
			if (GUILayout.Button("Reset Level"))
			{
				var game = (MapDungeonGame)target;
				game.ResetLevel();
			}
		}

		private void DisposeButton()
		{
			if (GUILayout.Button("Dispose"))
			{
				var game = (MapDungeonGame)target;
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

	[ExecuteInEditMode]
	public class MapDungeonGame : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private MapDungeonGameParams gameParams = new MapDungeonGameParams(1);

		[SerializeField]
		private GameState state = GameState.Unloaded;

		[SerializeField]
		private MapDungeonLevel level;

		[SerializeField]
		private Camera camera;

		private Character player;

		private Exit exit;

		[SerializeField]
		private Text levelLabel, stepsLeftLabel, stepsTakenLabel;

		private void Awake()
		{
			gameObject.isStatic = true;

			camera = FindObjectOfType<Camera>();
			Debug.Assert(camera);

			level = FindObjectOfType<MapDungeonLevel>();
			Debug.Assert(level);

			Debug.Assert(levelLabel);
			Debug.Assert(stepsLeftLabel);
			Debug.Assert(stepsTakenLabel);
		}

		#region Start

		private void Start()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
#endif
				LoadLevel();
#if UNITY_EDITOR
			}
#endif
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
			gameParams = new MapDungeonGameParams(gameParams.Level);
			camera.enabled = false;

			PlayerInputEnqueuer.Instance.LockInputs();

			level.GetComponent<MapDungeonActorSpawner>().Built += OnMapDungeonActorSpawnerBuilt;
			level.Built += OnMapDungeonLevelBuilt;

			level.Load(gameParams.mapDungeonLevelParams);

			StartCoroutine(BuildLevelCoroutine());
		}

		private IEnumerator BuildLevelCoroutine()
		{
			yield return 0;
			level.Build();
		}

		private void OnMapDungeonActorSpawnerBuilt(Type levelComponentType)
		{
			level.GetComponent<MapDungeonActorSpawner>().Built -= OnMapDungeonActorSpawnerBuilt;
			var actorSpawners = level.GetComponents<ActorSpawner>();

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
			gameParams.StepTaken();
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
				gameParams = new MapDungeonGameParams(gameParams.Level + 1);
				ReloadLevel();
			}
		}

		private void OnMapDungeonLevelBuilt(Type levelComponentBuiltType)
		{
			level.Built -= OnMapDungeonLevelBuilt;

			gameParams.mapDungeonLevelParams = level.MapDungeonLevelParams;

			StartCoroutine(StartLevel());
		}

		private IEnumerator StartLevel()
		{
			yield return 0;
			state = GameState.InGame;

			UpdateUI();
			var mapCenter = level.GetComponent<Map>().Center;
			camera.orthographicSize = Mathf.Min(mapCenter.x, mapCenter.y);
			SetCameraPosition(mapCenter);
			camera.enabled = true;

			gameParams.SetMaximumSteps(level.GetComponent<MapDungeon>(), level.GetComponent<MapDungeonActorSpawner>(), mapCenter);

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

				if (gameParams.StepsLeft == 0)
				{
					ResetLevel();
				}
				UpdateUI();
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
		}

		private void UpdateUI()
		{
			SetDungeonLevelLabel(gameParams.Level);
			SetStepsLeftLabel(gameParams.StepsLeft);
			SetStepsTakenLabel(MapDungeonGameParams.TotalStepsTaken);
		}

		private void SetDungeonLevelLabel(int level)
		{
			levelLabel.text = "Dungeon Level: " + level;
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
				/*SetCameraPosition(player.transform.position);
				camera.orthographicSize = Mathf.Min(Mathf.Clamp(Mathf.Abs(player.transform.position.x), 5, level.GetComponent<Map>().Center.x),
					Mathf.Clamp(Mathf.Abs(player.transform.position.y), 5, level.GetComponent<Map>().Center.y));*/
			}
		}

		private void SetCameraPosition(Vector3 position)
		{
			camera.transform.position = Vector3.back * 10f + position;
		}

		#endregion LateUpdate

		public void Dispose()
		{
			state = GameState.Unloaded;
			StopAllCoroutines();
			level.Dispose();
		}
	}
}