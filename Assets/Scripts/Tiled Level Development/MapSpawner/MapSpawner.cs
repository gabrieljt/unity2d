using System;
using UnityEngine;

namespace TiledLevel
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapSpawner))]
	public class MapSpawnerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build Spawners"))
			{
				var mapSpawner = (MapSpawner)target;
				IMapRoomParams mapRoomParams = new MapRoomParams(mapSpawner.MapRoom);
				IMapSpawnerParams mapSpawnerParams = new MapSpawnerParams();
				mapSpawner.Build(ref mapRoomParams, ref mapSpawnerParams);
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(MapRoom)
	)]
	public class MapSpawner : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private MapRoom mapRoom;

		public MapRoom MapRoom { get { return mapRoom; } }

		public Action<IMapSpawnerParams> Built = delegate { };

		private void Awake()
		{
			mapRoom = GetComponent<MapRoom>();
			mapRoom.Built += OnMapRoomBuilt;
		}

		private void OnMapRoomBuilt(IMapRoomParams mapRoomParams)
		{
			IMapSpawnerParams mapSpawnerParams = new MapSpawnerParams();
			Build(ref mapRoomParams, ref mapSpawnerParams);
		}

		public void Build(ref IMapRoomParams mapRoomParams, ref IMapSpawnerParams mapSpawnerParams)
		{
			BuildSpawners(mapRoomParams);
			Built(mapSpawnerParams);
		}

		public void DestroySpawners()
		{
			var actorSpawners = GetComponents<ActorSpawner>();

			for (int i = 0; i < actorSpawners.Length; i++)
			{
				DestroyImmediate(actorSpawners[i]);
			}
		}

		private void BuildSpawners(IMapRoomParams mapRoomParams)
		{
			DestroySpawners();

			bool playerSet = false, exitSet = false;
			for (int i = 0; i < mapRoomParams.Rooms.Length; i++)
			{
				MapRoom.Room room = mapRoomParams.Rooms[i];
				for (int x = 0; x < room.Width; x++)
				{
					for (int y = 0; y < room.Height; y++)
					{
						if (mapRoom.Map.Tiles[room.Left + x, room.Top + y].Type == TileType.Floor)
						{
							Vector2 tilePosition = new Vector2(room.Left + x, room.Top + y) + Vector2.one * 0.5f;
							switch (mapRoomParams.Rooms.Length)
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
									else if (i == mapRoomParams.Rooms.Length - 1)
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
			mapRoom.Built -= OnMapRoomBuilt;
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}