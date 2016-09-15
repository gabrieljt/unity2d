using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMap))]
public class TileMapInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Build"))
		{
			TileMap tileMap = (TileMap)target;
			tileMap.Build();
			FindObjectOfType<GameState>().OnTileMapBuilt();
		}
	}
}

[ExecuteInEditMode]
[RequireComponent(
	typeof(SpriteRenderer),
	typeof(Rigidbody2D)
)]
public class TileMap : MonoBehaviour
{
	[SerializeField]
	[Range(1, 128)]
	private int width;

	[SerializeField]
	[Range(1, 128)]
	private int height;

	[SerializeField]
	private int tileResolution = 16;

	[SerializeField]
	private Texture2D tilesetTexture;

	[SerializeField]
	private TilesetTile[] tilesetTiles;

	private SpriteRenderer spriteRenderer;

	new private Rigidbody2D rigidbody2D;

	private Tile[,] tiles;

	public Vector2 Origin { get { return new Vector2(width / 2f, height / 2f); } }

	public Action Built = delegate { };

	private void Awake()
	{
		Debug.Assert(tilesetTexture);
		Debug.Assert(tilesetTiles.Length > 0);

		spriteRenderer = GetComponent<SpriteRenderer>();

		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.isKinematic = true;

		gameObject.isStatic = true;
	}

	private void Start()
	{
		Build();
	}

	public void Build()
	{
		SetOrigin();

		BuildMap();

		BuildTexture();

		Built();
	}

	private void SetOrigin()
	{
		transform.position = Origin;
	}

	#region Build Map

	private void BuildMap()
	{
		tiles = new Tile[width, height];

		FillTiles(TileType.Water);

		BuildRooms();

		BuildCorridors();

		BuildWalls();

		BuildColliders();
	}

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

	private List<Room> rooms;

	private void FillTiles(TileType type)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				tiles[x, y] = new Tile(type);
			}
		}
	}

	private void BuildRooms()
	{
		rooms = new List<Room>();

		int attemptsLeft = 10;

		while (rooms.Count < 10 && attemptsLeft > 0)
		{
			int roomWidth = UnityEngine.Random.Range(4, 14);
			int roomHeight = UnityEngine.Random.Range(4, 10);

			Room room = new Room(
				new Rect(UnityEngine.Random.Range(0, width - roomWidth),
					UnityEngine.Random.Range(0, height - roomHeight),
					roomWidth,
					roomHeight)
			);

			if (!RoomCollides(room))
			{
				rooms.Add(room);
			}
			else
			{
				attemptsLeft--;
			}
		}

		foreach (Room room in rooms)
		{
			BuildRoom(room);
		}
	}

	private bool RoomCollides(Room room)
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

	private void BuildRoom(Room room)
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

	private void BuildCorridors()
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (!rooms[i].isConnected)
			{
				int j = UnityEngine.Random.Range(1, rooms.Count);
				BuildCorridor(rooms[i], rooms[(i + j) % rooms.Count]);
			}
		}
	}

	private void BuildCorridor(Room sourceRoom, Room targetRoom)
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

	private void BuildWalls()
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

	private void BuildColliders()
	{
		BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();

		for (int i = 0; i < colliders.Length; i++)
		{
			DestroyImmediate(colliders[i]);
		}

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (tiles[x, y].Type == TileType.Wall)
				{
					BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
					collider.offset = new Vector2(x, y) + Vector2.one * 0.5f - Origin;
					collider.size = Vector2.one;
				}
			}
		}
	}

	private bool HasAdjacentFloor(int x, int y)
	{
		return (x > 0 && tiles[x - 1, y].Type == TileType.Floor)
			|| (x < width - 1 && tiles[x + 1, y].Type == TileType.Floor)
			|| (y > 0 && tiles[x, y - 1].Type == TileType.Floor)
			|| (y < height - 1 && tiles[x, y + 1].Type == TileType.Floor)
			|| (x > 0 && y > 0 && tiles[x - 1, y - 1].Type == TileType.Floor)
			|| (x < width - 1 && y > 0 && tiles[x + 1, y - 1].Type == TileType.Floor)
			|| (x > 0 && y < height - 1 && tiles[x - 1, y + 1].Type == TileType.Floor)
			|| (x < width - 1 && y < height - 1 && tiles[x + 1, y + 1].Type == TileType.Floor);
	}

	public Vector2 GetRandomRoomCenter()
	{
		return rooms[UnityEngine.Random.Range(0, rooms.Count - 1)].Center;
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