using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Tiled
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeon))]
	public class MapDungeonInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build"))
			{
				var mapDungeon = (MapDungeon)target;
				mapDungeon.Build();
			}

			if (GUILayout.Button("Dispose"))
			{
				var mapDungeon = (MapDungeon)target;
				mapDungeon.Dispose();
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(Map)
	)]
	public class MapDungeon : ALevelComponent
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
		private int maximumDungeons = 50;

		[SerializeField]
		[Range(10, 100)]
		private int maximumAttempts = 100;

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		private void Awake()
		{
			map = GetComponent<Map>();
		}

		public override void Build()
		{
			BuildDungeon(ref map);

			Built();
		}

		private void DestroyRooms(ref Map map, ref Room[] rooms)
		{
			// TODO: clear corridors and walls
			if (map.tiles.Length > 0)
			{
				foreach (var room in rooms)
				{
					for (int x = 0; x < room.Width; x++)
					{
						for (int y = 0; y < room.Height; y++)
						{
							map.tiles[room.Left + x, room.Top + y] = new Tile(TileType.Water);
						}
					}
				}
			}
			rooms = new Room[0];
		}

		private void BuildDungeon(ref Map map)
		{
			BuildRooms(ref map, out rooms);

			BuildCorridors(ref map, ref rooms);

			BuildWalls(ref map, ref rooms);
		}

		private void BuildRooms(ref Map map, out Room[] rooms)
		{
			var roomsList = new List<Room>();
			var attemptsLeft = maximumAttempts;

			while (roomsList.Count < maximumDungeons && attemptsLeft > 0)
			{
				var dungeonWidth = (int)UnityEngine.Random.Range(4f, Mathf.Clamp(this.map.width * UnityEngine.Random.Range(0.1f, 0.35f), 4f, this.map.width * 0.35f));
				var dungeonHeight = (int)UnityEngine.Random.Range(3f, Mathf.Clamp(this.map.height * UnityEngine.Random.Range(0.1f, 0.35f), 3f, this.map.height * 0.35f));

				var dungeon = new Room(
					new Rect(UnityEngine.Random.Range(0, this.map.width - dungeonWidth),
						UnityEngine.Random.Range(0, this.map.height - dungeonHeight),
						dungeonWidth,
						dungeonHeight)
				);

				if (!RoomCollides(roomsList, dungeon))
				{
					roomsList.Add(dungeon);
				}
				else
				{
					attemptsLeft--;
				}
			}

			foreach (var room in roomsList)
			{
				BuildRoom(ref map, room);
			}

			Debug.Log(roomsList.Count + " rooms(s) built");

			rooms = roomsList.ToArray();
		}

		private bool RoomCollides(List<Room> rooms, Room room)
		{
			foreach (var otherDungeon in rooms)
			{
				if (room.CollidesWith(otherDungeon))
				{
					return true;
				}
			}

			return false;
		}

		private void BuildRoom(ref Map map, Room room)
		{
			for (int x = 0; x < room.Width; x++)
			{
				for (int y = 0; y < room.Height; y++)
				{
					if (x == 0 || x == room.Width - 1 || y == 0 || y == room.Height - 1)
					{
						map.tiles[room.Left + x, room.Top + y] = new Tile(TileType.Wall);
					}
					else
					{
						map.tiles[room.Left + x, room.Top + y] = new Tile(TileType.Floor);
					}
				}
			}
		}

		private void BuildCorridors(ref Map map, ref Room[] rooms)
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				if (!rooms[i].isConnected)
				{
					var j = UnityEngine.Random.Range(1, rooms.Length);
					BuildCorridor(ref map, ref rooms[i], ref rooms[(i + j) % rooms.Length]);
				}
			}
		}

		private void BuildCorridor(ref Map map, ref Room sourceRoom, ref Room targetRoom)
		{
			var x = sourceRoom.CenterX;
			var y = sourceRoom.CenterY;

			while (x != targetRoom.CenterX)
			{
				map.tiles[x, y] = new Tile(TileType.Floor);

				x += x < targetRoom.CenterX ? 1 : -1;
			}

			while (y != targetRoom.CenterY)
			{
				map.tiles[x, y] = new Tile(TileType.Floor);

				y += y < targetRoom.CenterY ? 1 : -1;
			}

			sourceRoom.isConnected = true;
			targetRoom.isConnected = true;
		}

		private void BuildWalls(ref Map map, ref Room[] rooms)
		{
			for (int x = 0; x < this.map.width; x++)
			{
				for (int y = 0; y < this.map.height; y++)
				{
					if (map.tiles[x, y].Type == TileType.Water && Map.HasAdjacentFloor(map, x, y))
					{
						map.tiles[x, y] = new Tile(TileType.Wall);
					}
				}
			}
		}

		public override void Dispose()
		{
			DestroyRooms(ref map, ref rooms);
		}
	}
}