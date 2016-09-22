using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapRoom))]
	public class MapRoomInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build Rooms"))
			{
				var roomMap = (MapRoom)target;
				IMapParams mapParams = new MapParams(roomMap.Map);
				IMapRoomParams roomMapParams = new MapRoomParams();
				roomMap.Build(ref mapParams, ref roomMapParams);
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(Map)
	)]
	public class MapRoom : MonoBehaviour, IDisposable
	{
		[Serializable]
		public class Room
		{
			public bool isConnected = false;

			[SerializeField]
			private Rect rect;

			public Room(Rect rect)
			{
				this.rect = rect;
			}

			public Rect Rect { get { return rect; } private set { rect = value; } }

			public int Width { get { return (int)Rect.width; } }

			public int Height { get { return (int)Rect.height; } }

			public int Left { get { return (int)Rect.xMin; } }

			public int Right { get { return (int)Rect.xMax; } }

			public int Top { get { return (int)Rect.yMin; } }

			public int Bottom { get { return (int)Rect.yMax; } }

			public int CenterX { get { return (int)Rect.center.x; } }

			public int CenterY { get { return (int)Rect.center.y; } }

			public Vector2 Center { get { return Rect.center; } }

			public bool CollidesWith(Room other)
			{
				return Rect.Overlaps(other.Rect);
			}
		}

		[SerializeField]
		private Room[] rooms = new Room[0];

		public Room[] Rooms { get { return rooms; } }

		[SerializeField]
		[Range(10, 50)]
		private int maximumRooms = 50;

		[SerializeField]
		[Range(10, 100)]
		private int maximumAttempts = 100;

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		public Action<IMapRoomParams> Built = delegate { };

		private void Awake()
		{
			map = GetComponent<Map>();
			map.Built += OnMapBuilt;
		}

		private void OnMapBuilt(IMapParams mapParams)
		{
			IMapRoomParams roomMapParams = new MapRoomParams();
			Build(ref mapParams, ref roomMapParams);
		}

		public void Build(ref IMapParams mapParams, ref IMapRoomParams roomMapParams)
		{
			if (rooms.Length > 0)
			{
				// TODO: clear corridors
				//ClearRooms(ref mapParams, ref rooms);

				// Hard Reset
				map.SetValues(mapParams);
				map.FillTiles(TileType.Water);
				mapParams.Tiles = map.Tiles;
			}

			BuildRooms(ref mapParams, out rooms);

			BuildCorridors(ref mapParams, ref rooms);

			BuildWalls(ref mapParams, ref rooms);

			map.UpdateValues(mapParams);

			roomMapParams = new MapRoomParams(this);

			Built(roomMapParams);
		}

		private void ClearRooms(ref IMapParams mapParams, ref Room[] rooms)
		{
			foreach (var room in rooms)
			{
				for (int x = 0; x < room.Width; x++)
				{
					for (int y = 0; y < room.Height; y++)
					{
						mapParams.Tiles[room.Left + x, room.Top + y] = new Tile(TileType.Water);
					}
				}
			}
			rooms = new Room[0];
		}

		private void BuildRooms(ref IMapParams mapParams, out Room[] rooms)
		{
			var roomList = new List<Room>();
			var attemptsLeft = maximumAttempts;

			while (roomList.Count < maximumRooms && attemptsLeft > 0)
			{
				var roomWidth = (int)UnityEngine.Random.Range(4f, Mathf.Clamp(map.Width * UnityEngine.Random.Range(0.1f, 0.35f), 4f, map.Width * 0.35f));
				var roomHeight = (int)UnityEngine.Random.Range(3f, Mathf.Clamp(map.Height * UnityEngine.Random.Range(0.1f, 0.35f), 3f, map.Height * 0.35f));

				var room = new Room(
					new Rect(UnityEngine.Random.Range(0, map.Width - roomWidth),
						UnityEngine.Random.Range(0, map.Height - roomHeight),
						roomWidth,
						roomHeight)
				);

				if (!RoomCollides(roomList, room))
				{
					roomList.Add(room);
				}
				else
				{
					attemptsLeft--;
				}
			}

			foreach (var room in roomList)
			{
				BuildRoom(ref mapParams, room);
			}

			Debug.Log(roomList.Count + " room(s) built");

			rooms = roomList.ToArray();
		}

		private bool RoomCollides(List<Room> rooms, Room room)
		{
			foreach (var otherRoom in rooms)
			{
				if (room.CollidesWith(otherRoom))
				{
					return true;
				}
			}

			return false;
		}

		private void BuildRoom(ref IMapParams mapParams, Room room)
		{
			for (int x = 0; x < room.Width; x++)
			{
				for (int y = 0; y < room.Height; y++)
				{
					if (x == 0 || x == room.Width - 1 || y == 0 || y == room.Height - 1)
					{
						mapParams.Tiles[room.Left + x, room.Top + y] = new Tile(TileType.Wall);
					}
					else
					{
						mapParams.Tiles[room.Left + x, room.Top + y] = new Tile(TileType.Floor);
					}
				}
			}
		}

		private void BuildCorridors(ref IMapParams mapParams, ref Room[] rooms)
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				if (!rooms[i].isConnected)
				{
					var j = UnityEngine.Random.Range(1, rooms.Length);
					BuildCorridor(ref mapParams, ref rooms[i], ref rooms[(i + j) % rooms.Length]);
				}
			}
		}

		private void BuildCorridor(ref IMapParams mapParams, ref Room sourceRoom, ref Room targetRoom)
		{
			var x = sourceRoom.CenterX;
			var y = sourceRoom.CenterY;

			while (x != targetRoom.CenterX)
			{
				mapParams.Tiles[x, y] = new Tile(TileType.Floor);

				x += x < targetRoom.CenterX ? 1 : -1;
			}

			while (y != targetRoom.CenterY)
			{
				mapParams.Tiles[x, y] = new Tile(TileType.Floor);

				y += y < targetRoom.CenterY ? 1 : -1;
			}

			sourceRoom.isConnected = true;
			targetRoom.isConnected = true;
		}

		private void BuildWalls(ref IMapParams mapParams, ref Room[] rooms)
		{
			for (int x = 0; x < map.Width; x++)
			{
				for (int y = 0; y < map.Height; y++)
				{
					if (mapParams.Tiles[x, y].Type == TileType.Water && Map.HasAdjacentFloor(mapParams, x, y))
					{
						mapParams.Tiles[x, y] = new Tile(TileType.Wall);
					}
				}
			}
		}

		public void Dispose()
		{
			map.Built += OnMapBuilt;
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}