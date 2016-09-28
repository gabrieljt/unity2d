using Game.Actor;
using Game.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Tiled
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeonActorSpawner))]
	public class MapDungeonActorSpawnerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build"))
			{
				var mapDungeonActorSpawner = (MapDungeonActorSpawner)target;
				mapDungeonActorSpawner.Build();
			}

			if (GUILayout.Button("Dispose"))
			{
				var mapDungeonSpawner = (MapDungeonActorSpawner)target;
				mapDungeonSpawner.Dispose();
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(MapDungeon)
	)]
	public class MapDungeonActorSpawner : MonoBehaviour, IBuildable, IDestroyable, IDisposable
	{
		// TODO: better input params
		private Dictionary<ActorType, List<AActor>> spawnedActors = new Dictionary<ActorType, List<AActor>>();

		[SerializeField]
		private MapDungeon mapDungeon;

		public MapDungeon MapDungeon { get { return mapDungeon; } }

		private Action built = delegate { };

		public Action Built { get { return built; } set { built = value; } }

		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		private void Awake()
		{
			mapDungeon = GetComponent<MapDungeon>();
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
					Destroy(actor.gameObject);
				}
				actorsList.Clear();
			}
		}

		public void Build()
		{
			if (spawnedActors.Count == 0)
			{
				SetSpawnedActorsLists();
			}
			BuildActorSpawners();
			Built();
		}

		public void DestroyActorSpawners()
		{
			var actorSpawners = GetComponents<ActorSpawner>();
			for (int i = 0; i < actorSpawners.Length; i++)
			{
				Destroy(actorSpawners[i]);
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
		private void SetPlayerSpawner(ref bool playerSet, Vector2 position)
		{
			if (!playerSet)
			{
				BuildActorSpawner(position, ActorType.Player);
				playerSet = true;
			}
		}

		// TODO: decouple game rules
		private void SetExitSpawner(ref bool exitSet, Vector2 position)
		{
			if (!exitSet)
			{
				BuildActorSpawner(position, ActorType.Exit);
				exitSet = true;
			}
		}

		private void BuildActorSpawner(Vector2 position, ActorType actorType)
		{
			var actorSpawner = gameObject.AddComponent<ActorSpawner>();
			actorSpawner.actorType = actorType;
			actorSpawner.position = position;
			actorSpawner.Spawned += OnActorSpawned;
		}

		private void OnActorSpawned(ActorSpawner actorSpawner, AActor spawnedActor)
		{
			Debug.LogWarning(spawnedActor.GetType() + " spawned");

			actorSpawner.Spawned -= OnActorSpawned;

			if (actorSpawner.IsType<Character>() && spawnedActor.CompareTag(ActorType.Player.ToString()))
			{
				var playerCharacterInputDequeuer = spawnedActor.GetComponent<CharacterInputDequeuer>() as AInputDequeuer;
				PlayerInputEnqueuer.Add(ref playerCharacterInputDequeuer);
			}

			spawnedActor.transform.SetParent(transform);
			spawnedActors[actorSpawner.actorType].Add(spawnedActor);
		}

		public void Dispose()
		{
			DestroyActorSpawners();
			ClearSpawnedActorsLists();
			spawnedActors.Clear();
		}

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}