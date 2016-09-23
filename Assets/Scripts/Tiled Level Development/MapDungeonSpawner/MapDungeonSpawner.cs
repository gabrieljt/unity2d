using System;
using UnityEngine;

namespace TiledLevel
{
	using System.Collections.Generic;

#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeonSpawner))]
	public class MapDungeonSpawnerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build Spawners"))
			{
				var mapDungeonSpawner = (MapDungeonSpawner)target;
				IMapDungeonParams mapDungeonParams = new MapDungeonParams(mapDungeonSpawner.MapDungeon);
				IMapDungeonSpawnerParams mapDungeonSpawnerParams = new MapDungeonSpawnerParams();
				mapDungeonSpawner.Build(ref mapDungeonParams, ref mapDungeonSpawnerParams);
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(MapDungeon)
	)]
	public class MapDungeonSpawner : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private MapDungeon mapDungeon;

		public MapDungeon MapDungeon { get { return mapDungeon; } }

		public Action<IMapDungeonSpawnerParams> Built = delegate { };

		private Dictionary<ActorType, List<GameObject>> actors = new Dictionary<ActorType, List<GameObject>>();

		private void Awake()
		{
			mapDungeon = GetComponent<MapDungeon>();
			mapDungeon.Built += OnMapDungeonBuilt;

			SetActorsLists();
		}

		private void SetActorsLists()
		{
			var actorTypes = Enum.GetValues(typeof(ActorType));
			foreach (var actorType in actorTypes)
			{
				actors[(ActorType)actorType] = new List<GameObject>();
			}
		}

		private void ClearActorsLists()
		{
			foreach (var actorsList in actors.Values)
			{
				foreach (var actor in actorsList)
				{
					DestroyImmediate(actor);
				}
				actorsList.Clear();
			}
		}

		private void OnMapDungeonBuilt(IMapDungeonParams mapDungeonParams)
		{
			IMapDungeonSpawnerParams mapSpawnerParams = new MapDungeonSpawnerParams();
			Build(ref mapDungeonParams, ref mapSpawnerParams);
		}

		public void Build(ref IMapDungeonParams mapDungeonParams, ref IMapDungeonSpawnerParams mapDungeonSpawnerParams)
		{
			if (actors.Count == 0)
			{
				SetActorsLists();
			}

			ClearActorsLists();
			DestroySpawners();

			BuildSpawners(mapDungeonParams);
			Built(mapDungeonSpawnerParams);
		}

		public void DestroySpawners()
		{
			var actorSpawners = GetComponents<ActorSpawner>();

			for (int i = 0; i < actorSpawners.Length; i++)
			{
				DestroyImmediate(actorSpawners[i]);
			}
		}

		private void BuildSpawners(IMapDungeonParams mapDungeonParams)
		{
			bool playerSet = false, exitSet = false;
			for (int i = 0; i < mapDungeonParams.Dungeons.Length; i++)
			{
				MapDungeon.Room dungeon = mapDungeonParams.Dungeons[i];
				for (int x = 0; x < dungeon.Width; x++)
				{
					for (int y = 0; y < dungeon.Height; y++)
					{
						if (mapDungeon.Map.Tiles[dungeon.Left + x, dungeon.Top + y].Type == TileType.Floor)
						{
							Vector2 tilePosition = new Vector2(dungeon.Left + x, dungeon.Top + y) + Vector2.one * 0.5f;
							switch (mapDungeonParams.Dungeons.Length)
							{
								case 1:
									SetPlayerSpawner(ref playerSet, tilePosition);
									SetExitSpawner(ref exitSet, tilePosition);
									break;

								default:
									if (i == 0)
									{
										SetPlayerSpawner(ref playerSet, tilePosition);
									}
									else if (i == mapDungeonParams.Dungeons.Length - 1)
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

		private void SetPlayerSpawner(ref bool playerSet, Vector2 tilePosition)
		{
			if (!playerSet)
			{
				BuildSpawner(tilePosition, ActorType.Character);
				playerSet = true;
			}
		}

		private void SetExitSpawner(ref bool exitSet, Vector2 tilePosition)
		{
			if (!exitSet)
			{
				BuildSpawner(tilePosition, ActorType.Exit);
				exitSet = true;
			}
		}

		private void BuildSpawner(Vector2 tilePosition, ActorType actorType)
		{
			var actorSpawner = gameObject.AddComponent<ActorSpawner>();
			actorSpawner.actorType = actorType;
			actorSpawner.position = tilePosition;
			actorSpawner.Spawned += OnActorSpawned;
		}

		private void OnActorSpawned(ActorSpawner actorSpawner, GameObject spawnedActor)
		{
			if (actorSpawner.IsType<Character>())
			{
				Debug.LogWarning(spawnedActor.GetType() + " spawned");
			}

			actorSpawner.Spawned -= OnActorSpawned;

			spawnedActor.transform.SetParent(transform);
			actors[actorSpawner.actorType].Add(spawnedActor);
		}

		public void Dispose()
		{
			mapDungeon.Built -= OnMapDungeonBuilt;
			ClearActorsLists();
			actors.Clear();
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}