using System;
using System.Collections.Generic;
using UnityEngine;

namespace TiledLevel
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeon))]
	public class MapDungeonInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build Dungeons"))
			{
				var mapDungeon = (MapDungeon)target;
				IMapParams mapParams = new MapParams(mapDungeon.Map);
				IMapDungeonParams mapDungeonParams = new MapDungeonParams();
				mapDungeon.Build(ref mapParams, ref mapDungeonParams);
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(Map)
	)]
	public class MapDungeon : MonoBehaviour, IDisposable
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
		private Room[] dungeons = new Room[0];

		public Room[] Dungeons { get { return dungeons; } }

		[SerializeField]
		[Range(10, 50)]
		private int maximumDungeons = 50;

		[SerializeField]
		[Range(10, 100)]
		private int maximumAttempts = 100;

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		public Action<IMapDungeonParams> Built = delegate { };

		private void Awake()
		{
			map = GetComponent<Map>();
			map.Built += OnMapBuilt;
		}

		private void OnMapBuilt(IMapParams mapParams)
		{
			IMapDungeonParams mapDungeonParams = new MapDungeonParams();
			Build(ref mapParams, ref mapDungeonParams);
		}

		public void Build(ref IMapParams mapParams, ref IMapDungeonParams mapDungeonParams)
		{
			if (dungeons.Length > 0)
			{
				// TODO: clear corridors
				//ClearDungeons(ref mapParams, ref dungeons);

				// Hard Reset
				map.SetValues(mapParams);
				map.FillTiles(TileType.Water);
				mapParams.Tiles = map.Tiles;
			}

			BuildDungeons(ref mapParams, out dungeons);

			BuildCorridors(ref mapParams, ref dungeons);

			BuildWalls(ref mapParams, ref dungeons);

			map.UpdateValues(mapParams);

			mapDungeonParams = new MapDungeonParams(this);

			Built(mapDungeonParams);
		}

		private void ClearDungeons(ref IMapParams mapParams, ref Room[] dungeons)
		{
			foreach (var dungeon in dungeons)
			{
				for (int x = 0; x < dungeon.Width; x++)
				{
					for (int y = 0; y < dungeon.Height; y++)
					{
						mapParams.Tiles[dungeon.Left + x, dungeon.Top + y] = new Tile(TileType.Water);
					}
				}
			}
			dungeons = new Room[0];
		}

		private void BuildDungeons(ref IMapParams mapParams, out Room[] dungeons)
		{
			var dungeonList = new List<Room>();
			var attemptsLeft = maximumAttempts;

			while (dungeonList.Count < maximumDungeons && attemptsLeft > 0)
			{
				var dungeonWidth = (int)UnityEngine.Random.Range(4f, Mathf.Clamp(map.Width * UnityEngine.Random.Range(0.1f, 0.35f), 4f, map.Width * 0.35f));
				var dungeonHeight = (int)UnityEngine.Random.Range(3f, Mathf.Clamp(map.Height * UnityEngine.Random.Range(0.1f, 0.35f), 3f, map.Height * 0.35f));

				var dungeon = new Room(
					new Rect(UnityEngine.Random.Range(0, map.Width - dungeonWidth),
						UnityEngine.Random.Range(0, map.Height - dungeonHeight),
						dungeonWidth,
						dungeonHeight)
				);

				if (!DungeonCollides(dungeonList, dungeon))
				{
					dungeonList.Add(dungeon);
				}
				else
				{
					attemptsLeft--;
				}
			}

			foreach (var dungeon in dungeonList)
			{
				BuildDungeon(ref mapParams, dungeon);
			}

			Debug.Log(dungeonList.Count + " dungeon(s) built");

			dungeons = dungeonList.ToArray();
		}

		private bool DungeonCollides(List<Room> dungeons, Room dungeon)
		{
			foreach (var otherDungeon in dungeons)
			{
				if (dungeon.CollidesWith(otherDungeon))
				{
					return true;
				}
			}

			return false;
		}

		private void BuildDungeon(ref IMapParams mapParams, Room dungeon)
		{
			for (int x = 0; x < dungeon.Width; x++)
			{
				for (int y = 0; y < dungeon.Height; y++)
				{
					if (x == 0 || x == dungeon.Width - 1 || y == 0 || y == dungeon.Height - 1)
					{
						mapParams.Tiles[dungeon.Left + x, dungeon.Top + y] = new Tile(TileType.Wall);
					}
					else
					{
						mapParams.Tiles[dungeon.Left + x, dungeon.Top + y] = new Tile(TileType.Floor);
					}
				}
			}
		}

		private void BuildCorridors(ref IMapParams mapParams, ref Room[] dungeons)
		{
			for (int i = 0; i < dungeons.Length; i++)
			{
				if (!dungeons[i].isConnected)
				{
					var j = UnityEngine.Random.Range(1, dungeons.Length);
					BuildCorridor(ref mapParams, ref dungeons[i], ref dungeons[(i + j) % dungeons.Length]);
				}
			}
		}

		private void BuildCorridor(ref IMapParams mapParams, ref Room sourceDungeon, ref Room targetDungeon)
		{
			var x = sourceDungeon.CenterX;
			var y = sourceDungeon.CenterY;

			while (x != targetDungeon.CenterX)
			{
				mapParams.Tiles[x, y] = new Tile(TileType.Floor);

				x += x < targetDungeon.CenterX ? 1 : -1;
			}

			while (y != targetDungeon.CenterY)
			{
				mapParams.Tiles[x, y] = new Tile(TileType.Floor);

				y += y < targetDungeon.CenterY ? 1 : -1;
			}

			sourceDungeon.isConnected = true;
			targetDungeon.isConnected = true;
		}

		private void BuildWalls(ref IMapParams mapParams, ref Room[] dungeons)
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
			map.Built -= OnMapBuilt;
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}