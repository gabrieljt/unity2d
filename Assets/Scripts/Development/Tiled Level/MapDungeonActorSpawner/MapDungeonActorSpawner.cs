using Actor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TiledLevel
{
	using Input;

#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeonActorSpawner))]
	public class MapDungeonActorSpawnerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build Actor Spawners"))
			{
				var mapDungeonActorSpawner = (MapDungeonActorSpawner)target;
				mapDungeonActorSpawner.Build();
			}

			if (GUILayout.Button("Destroy Actor Spawners"))
			{
				var mapDungeonSpawner = (MapDungeonActorSpawner)target;
				mapDungeonSpawner.DestroyActorSpawners();
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(MapDungeon)
	)]
	public class MapDungeonActorSpawner : MonoBehaviour, IDisposable
	{
		// TODO: better input params

		[SerializeField]
		private MapDungeon mapDungeon;

		public MapDungeon MapDungeon { get { return mapDungeon; } }

		public Action Built = delegate { };

		private Dictionary<ActorType, List<AActor>> spawnedActors = new Dictionary<ActorType, List<AActor>>();

		private void Awake()
		{
			mapDungeon = GetComponent<MapDungeon>();
			mapDungeon.Built += OnMapDungeonBuilt;

			SetSpawnedActorsLists();
		}

		private void SetSpawnedActorsLists()
		{
			var actorTypes = Enum.GetValues(typeof(ActorType));
			foreach (var actorType in actorTypes)
			{
				spawnedActors[(ActorType)actorType] = new List<AActor>();
			}
		}

		private void ClearSpawnedActorsLists()
		{
			foreach (var actorsList in spawnedActors.Values)
			{
				foreach (var actor in actorsList)
				{
					DestroyImmediate(actor.gameObject);
				}
				actorsList.Clear();
			}
		}

		private void OnMapDungeonBuilt()
		{
			Build();
		}

		public void Build()
		{
			DestroyActorSpawners();

			BuildActorSpawners();
			Built();
		}

		public void DestroyActorSpawners()
		{
			if (spawnedActors.Count == 0)
			{
				SetSpawnedActorsLists();
			}

			ClearSpawnedActorsLists();
			var actorSpawners = GetComponents<ActorSpawner>();

			for (int i = 0; i < actorSpawners.Length; i++)
			{
				DestroyImmediate(actorSpawners[i]);
			}
		}

		private void BuildActorSpawners()
		{
			bool playerSet = false, exitSet = false;
			for (int i = 0; i < mapDungeon.Rooms.Length; i++)
			{
				MapDungeon.Room dungeon = mapDungeon.Rooms[i];
				for (int x = 0; x < dungeon.Width; x++)
				{
					for (int y = 0; y < dungeon.Height; y++)
					{
						if (mapDungeon.Map.Tiles[dungeon.Left + x, dungeon.Top + y].Type == TileType.Floor)
						{
							Vector2 tilePosition = new Vector2(dungeon.Left + x, dungeon.Top + y) + Vector2.one * 0.5f;
							switch (mapDungeon.Rooms.Length)
							{
								// TODO: decouple game rules
								case 1:
									SetPlayerSpawner(ref playerSet, tilePosition);
									SetExitSpawner(ref exitSet, tilePosition);
									break;

								default:
									if (i == 0)
									{
										SetPlayerSpawner(ref playerSet, tilePosition);
									}
									else if (i == mapDungeon.Rooms.Length - 1)
									{
										SetExitSpawner(ref exitSet, tilePosition);
									}

									break;
							}
							if (playerSet && exitSet)
							{
								return;
							}
						}
					}
				}
			}
		}

		// TODO: decouple game rules
		private void SetPlayerSpawner(ref bool playerSet, Vector2 tilePosition)
		{
			if (!playerSet)
			{
				BuildActorSpawner(tilePosition, ActorType.Player);
				playerSet = true;
			}
		}

		// TODO: decouple game rules
		private void SetExitSpawner(ref bool exitSet, Vector2 tilePosition)
		{
			if (!exitSet)
			{
				BuildActorSpawner(tilePosition, ActorType.Exit);
				exitSet = true;
			}
		}

		private void BuildActorSpawner(Vector2 tilePosition, ActorType actorType)
		{
			var actorSpawner = gameObject.AddComponent<ActorSpawner>();
			actorSpawner.actorType = actorType;
			actorSpawner.position = tilePosition;
			actorSpawner.Spawned += OnActorSpawned;
		}

		private void OnActorSpawned(ActorSpawner actorSpawner, AActor spawnedActor)
		{
			Debug.LogWarning(spawnedActor.GetType() + " spawned");

			actorSpawner.Spawned -= OnActorSpawned;

			if (actorSpawner.IsType<Character>() && spawnedActor.CompareTag("Player"))
			{
				PlayerInputEnqueuer.SetInputDequeuer(spawnedActor.GetComponent<CharacterInputDequeuer>());
			}

			spawnedActor.transform.SetParent(transform);
			spawnedActors[actorSpawner.actorType].Add(spawnedActor);
		}

		public void Dispose()
		{
			mapDungeon.Built -= OnMapDungeonBuilt;
			ClearSpawnedActorsLists();
			spawnedActors.Clear();
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}