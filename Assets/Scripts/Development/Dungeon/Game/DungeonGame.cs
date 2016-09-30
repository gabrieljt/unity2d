using Dungeon.Game.Level;
using Dungeon.Game.TileMap;
using Game.Actor;
using Game.Input;
using Game.TileMap;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Game
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(DungeonGame))]
	public class DungeonGameInspector : Editor
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
				var game = (DungeonGame)target;
				game.LoadLevel();
			}
		}

		private void ResetLevelButton()
		{
			if (GUILayout.Button("Reset Level"))
			{
				var game = (DungeonGame)target;
				game.ResetLevel();
			}
		}

		private void DisposeButton()
		{
			if (GUILayout.Button("Dispose"))
			{
				var game = (DungeonGame)target;
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
	public class DungeonGame : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private DungeonGameParams @params = new DungeonGameParams(1);

		[SerializeField]
		private GameState state = GameState.Unloaded;

		[SerializeField]
		private DungeonLevel level;

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

			level = FindObjectOfType<DungeonLevel>();
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
			@params = new DungeonGameParams(@params.Level);
			camera.enabled = false;

			PlayerInputEnqueuer.Instance.LockInputs();

			level.GetComponent<MapActorSpawners>().Built += OnSpawnersBuilt;
			level.Built += OnLevelBuilt;

			level.Load(@params.levelParams);

			StartCoroutine(BuildLevelCoroutine());
		}

		private IEnumerator BuildLevelCoroutine()
		{
			yield return 0;
			level.Build();
		}

		private void OnSpawnersBuilt(Type type)
		{
			level.GetComponent<MapActorSpawners>().Built -= OnSpawnersBuilt;
			var spawners = level.GetComponents<ActorSpawner>();

			foreach (var spawner in spawners)
			{
				spawner.Performed += OnSpawnerPerformed;
				if (spawner.type == ActorType.Player)
				{
					spawner.Performed += OnPlayerSpawned;
				}

				if (spawner.type == ActorType.Exit)
				{
					spawner.Performed += OnExitSpawned;
				}
			}
		}

		private void OnSpawnerPerformed(ActorSpawner spawner, AActor actor)
		{
			spawner.Performed -= OnSpawnerPerformed;
			spawner.enabled = false;
		}

		private void OnPlayerSpawned(ActorSpawner spawner, AActor actor)
		{
			spawner.Performed -= OnPlayerSpawned;
			player = actor as Character;
			player.StepTaken += OnStepTaken;
			player.Destroyed += OnPlayerDestroyed;

			var inputDequeuer = player.GetComponent<CharacterInputDequeuer>() as AInputDequeuer;
			PlayerInputEnqueuer.Add(ref inputDequeuer);
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
				@params = new DungeonGameParams(@params.Level + 1);
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

			UpdateUI();
			var mapCenter = level.GetComponent<Map>().Center;
			camera.orthographicSize = Mathf.Min(mapCenter.x, mapCenter.y);
			SetCameraPosition(mapCenter);
			camera.enabled = true;

			@params.SetMaximumSteps(level.GetComponent<DungeonMap>(), level.GetComponent<MapActorSpawners>());

			PlayerInputEnqueuer.Instance.UnlockInputs();
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
			SetLevelLabel(@params.Level);
			SetStepsLeftLabel(@params.StepsLeft);
			SetStepsTakenLabel(DungeonGameParams.TotalStepsTaken);
		}

		private void SetLevelLabel(int level)
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
				/*
				SetCameraPosition(player.transform.position);
				camera.orthographicSize = Mathf.Min(Mathf.Clamp(Mathf.Abs(player.transform.position.x), 5, level.GetComponent<Map>().Center.x),
					Mathf.Clamp(Mathf.Abs(player.transform.position.y), 5, level.GetComponent<Map>().Center.y));
				*/
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
			level.DisposeAll();
		}
	}
}