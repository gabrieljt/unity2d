using System;
using UnityEngine;

namespace TiledLevel
{
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

		private void Awake()
		{
			mapDungeon = GetComponent<MapDungeon>();
			mapDungeon.Built += OnMapDungeonBuilt;
		}

		private void OnMapDungeonBuilt(IMapDungeonParams mapDungeonParams)
		{
			IMapDungeonSpawnerParams mapSpawnerParams = new MapDungeonSpawnerParams();
			Build(ref mapDungeonParams, ref mapSpawnerParams);
		}

		public void Build(ref IMapDungeonParams mapDungeonParams, ref IMapDungeonSpawnerParams mapDungeonSpawnerParams)
		{
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
			DestroySpawners();

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
				SetPlayerPosition(tilePosition);
				playerSet = true;
			}
		}

		private void SetExitSpawner(ref bool exitSet, Vector2 tilePosition)
		{
			if (!exitSet)
			{
				SetExitPosition(tilePosition);
				exitSet = true;
			}
		}

		private void SetPlayerPosition(Vector2 tilePosition)
		{
			var actorSpawner = gameObject.AddComponent<ActorSpawner>();
			actorSpawner.actorType = ActorType.Character;
			actorSpawner.position = tilePosition;
		}

		private void SetExitPosition(Vector2 tilePosition)
		{
			var actorSpawner = gameObject.AddComponent<ActorSpawner>();
			actorSpawner.actorType = ActorType.Exit;
			actorSpawner.position = tilePosition;
		}

		public void Dispose()
		{
			mapDungeon.Built -= OnMapDungeonBuilt;
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}