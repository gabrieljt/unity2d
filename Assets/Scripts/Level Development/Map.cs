using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(Map))]
	public class MapInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build Level"))
			{
				var map = (Map)target;
				IMapParams mapParams = new MapParams();
				mapParams.Height = map.Height;
				mapParams.Width = map.Width;
				map.Build(ref mapParams);
			}

			if (GUILayout.Button("Dispose Colliders"))
			{
				Map map = (Map)target;
				map.DisposeColliders();
			}

			if (GUILayout.Button("Build Colliders"))
			{
				Map map = (Map)target;
				map.BuildColliders();
			}
		}
	}

#endif

	/// <summary>
	/// TODO: externalize texture handling
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(
		typeof(SpriteRenderer)
	)]
	public class Map : MonoBehaviour
	{
		public class Room
		{
			public bool isConnected = false;

			public Room(Rect rect)
			{
				Rect = rect;
			}

			public Rect Rect { get; private set; }

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
		private SpriteRenderer spriteRenderer;

		[SerializeField]
		[Range(4, 128)]
		private int width = 16;

		public int Width { get { return width; } }

		[SerializeField]
		[Range(3, 128)]
		private int height = 9;

		public int Height { get { return height; } }

		[SerializeField]
		private Room[] rooms;

		public Room[] Rooms { get { return rooms; } }

		[SerializeField]
		[Range(10, 50)]
		private int maximumRooms = 10;

		[SerializeField]
		[Range(10, 50)]
		private int maximumAttempts = 10;

		private Tile[,] tiles;

		public Tile[,] Tiles { get { return tiles; } }

		public Vector2 WorldPosition { get { return new Vector2(width / 2f, height / 2f); } }

		[SerializeField]
		private int tileResolution = 16;

		[SerializeField]
		private Texture2D tilesetTexture;

		[SerializeField]
		private TilesetTile[] tilesetTiles;

		public Action Built = delegate { };

		private void Awake()
		{
			Debug.Assert(tilesetTexture);
			Debug.Assert(tilesetTiles.Length > 0);

			spriteRenderer = GetComponent<SpriteRenderer>();
			gameObject.isStatic = true;
		}

		public void Build(ref IMapParams mapParams)
		{
			DisposeColliders();

			this.width = mapParams.Width;
			this.height = mapParams.Height;

			SetWorldPosition();

			this.tiles = new Tile[width, height];
			FillTiles(tiles, TileType.Water);

			BuildRooms(ref tiles, out rooms);

			BuildCorridors(ref tiles, ref rooms);

			BuildWalls(ref tiles, ref rooms);

			BuildColliders();

			mapParams.Tiles = tiles;
			mapParams.Rooms = rooms;

			BuildTexture();

			Built();
		}

		private void SetWorldPosition()
		{
			transform.position = WorldPosition;
		}

		#region Build Map

		private void FillTiles(Tile[,] tiles, TileType type)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					tiles[x, y] = new Tile(type);
				}
			}
		}

		private void BuildRooms(ref Tile[,] tiles, out Room[] rooms)
		{
			List<Room> roomList = new List<Room>();

			int attemptsLeft = maximumAttempts;

			while (roomList.Count < maximumRooms && attemptsLeft > 0)
			{
				int roomWidth = (int)UnityEngine.Random.Range(4f, Mathf.Clamp(width * UnityEngine.Random.Range(0.1f, 0.35f), 4f, width * 0.35f));
				int roomHeight = (int)UnityEngine.Random.Range(3f, Mathf.Clamp(height * UnityEngine.Random.Range(0.1f, 0.35f), 3f, height * 0.35f));

				Room room = new Room(
					new Rect(UnityEngine.Random.Range(0, width - roomWidth),
						UnityEngine.Random.Range(0, height - roomHeight),
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

			foreach (Room room in roomList)
			{
				BuildRoom(ref tiles, room);
			}

			Debug.Log(roomList.Count + " room(s) built");

			rooms = roomList.ToArray();
		}

		private bool RoomCollides(List<Room> rooms, Room room)
		{
			foreach (Room otherRoom in rooms)
			{
				if (room.CollidesWith(otherRoom))
				{
					return true;
				}
			}

			return false;
		}

		private void BuildRoom(ref Tile[,] tiles, Room room)
		{
			for (int x = 0; x < room.Width; x++)
			{
				for (int y = 0; y < room.Height; y++)
				{
					if (x == 0 || x == room.Width - 1 || y == 0 || y == room.Height - 1)
					{
						tiles[room.Left + x, room.Top + y] = new Tile(TileType.Wall);
					}
					else
					{
						tiles[room.Left + x, room.Top + y] = new Tile(TileType.Floor);
					}
				}
			}
		}

		private void BuildCorridors(ref Tile[,] tiles, ref Room[] rooms)
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				if (!rooms[i].isConnected)
				{
					int j = UnityEngine.Random.Range(1, rooms.Length);
					BuildCorridor(ref tiles, ref rooms[i], ref rooms[(i + j) % rooms.Length]);
				}
			}
		}

		private void BuildCorridor(ref Tile[,] tiles, ref Room sourceRoom, ref Room targetRoom)
		{
			int x = sourceRoom.CenterX;
			int y = sourceRoom.CenterY;

			while (x != targetRoom.CenterX)
			{
				tiles[x, y] = new Tile(TileType.Floor);

				x += x < targetRoom.CenterX ? 1 : -1;
			}

			while (y != targetRoom.CenterY)
			{
				tiles[x, y] = new Tile(TileType.Floor);

				y += y < targetRoom.CenterY ? 1 : -1;
			}

			sourceRoom.isConnected = true;
			targetRoom.isConnected = true;
		}

		private void BuildWalls(ref Tile[,] tiles, ref Room[] rooms)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (tiles[x, y].Type == TileType.Water && HasAdjacentFloor(x, y))
					{
						tiles[x, y] = new Tile(TileType.Wall);
					}
				}
			}
		}

		public void DisposeColliders()
		{
			BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();

			for (int i = 0; i < colliders.Length; i++)
			{
				DestroyImmediate(colliders[i]);
			}
		}

		public void BuildColliders()
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (tiles[x, y].Type == TileType.Wall)
					{
						BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
						collider.offset = new Vector2(x, y) + Vector2.one * 0.5f - WorldPosition;
						collider.size = Vector2.one;
					}
				}
			}
		}

		private bool HasAdjacentFloor(int x, int y)
		{
			return HasAdjacentType(x, y, TileType.Floor);
		}

		private bool HasAdjacentType(int x, int y, TileType type)
		{
			return (x > 0 && tiles[x - 1, y].Type == type)
				|| (x < width - 1 && tiles[x + 1, y].Type == type)
				|| (y > 0 && tiles[x, y - 1].Type == type)
				|| (y < height - 1 && tiles[x, y + 1].Type == type)
				|| (x > 0 && y > 0 && tiles[x - 1, y - 1].Type == type)
				|| (x < width - 1 && y > 0 && tiles[x + 1, y - 1].Type == type)
				|| (x > 0 && y < height - 1 && tiles[x - 1, y + 1].Type == type)
				|| (x < width - 1 && y < height - 1 && tiles[x + 1, y + 1].Type == type);
		}

		private bool HasAdjacentType(int x, int y, TileType type, out List<Tile> tiles)
		{
			tiles = new List<Tile>();
			if (x > 0 && this.tiles[x - 1, y].Type == type)
			{
				tiles.Add(this.tiles[x - 1, y]);
			}

			if (x < width - 1 && this.tiles[x + 1, y].Type == type)
			{
				tiles.Add(this.tiles[x + 1, y]);
			}

			if (y > 0 && this.tiles[x, y - 1].Type == type)
			{
				tiles.Add(this.tiles[x, y - 1]);
			}

			if (y < height - 1 && this.tiles[x, y + 1].Type == type)
			{
				tiles.Add(this.tiles[x, y + 1]);
			}

			if (x > 0 && y > 0 && this.tiles[x - 1, y - 1].Type == type)
			{
				tiles.Add(this.tiles[x - 1, y - 1]);
			}

			if (x < width - 1 && y > 0 && this.tiles[x + 1, y - 1].Type == type)
			{
				tiles.Add(this.tiles[x + 1, y - 1]);
			}

			if (x > 0 && y < height - 1 && this.tiles[x - 1, y + 1].Type == type)
			{
				tiles.Add(this.tiles[x - 1, y + 1]);
			}

			if (x < width - 1 && y < height - 1 && this.tiles[x + 1, y + 1].Type == type)
			{
				tiles.Add(this.tiles[x + 1, y + 1]);
			}

			return tiles.Count > 0;
		}

		#endregion Build Map

		#region Build Texture

		private Color[][] GetPixelsFromTexture()
		{
			int tilesPerRow = tilesetTexture.width / tileResolution;
			int rows = tilesetTexture.height / tileResolution;

			Color[][] tilesPixels = new Color[tilesPerRow * rows][];

			for (int y = 0; y < rows; y++)
			{
				for (int x = 0; x < tilesPerRow; x++)
				{
					tilesPixels[y * tilesPerRow + x] = tilesetTexture.GetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution);
				}
			}

			return tilesPixels;
		}

		public int GetTilesetTileIndexByType(TileType type)
		{
			return System.Array.Find(tilesetTiles, tilesetTile => tilesetTile.Type == type).TilesetIndex;
		}

		private void BuildTexture()
		{
			int textureWidth = width * tileResolution;
			int textureHeight = height * tileResolution;
			Texture2D texture = new Texture2D(textureWidth, textureHeight);

			Color[][] tilesPixels = GetPixelsFromTexture();

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color[] pixels = tilesPixels[GetTilesetTileIndexByType(tiles[x, y].Type)];
					texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, pixels);
				}
			}

			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.Apply();

			spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f, tileResolution);
		}

		#endregion Build Texture
	}
}