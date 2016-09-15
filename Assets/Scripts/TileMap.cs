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
			tileMap.SetupGameObject();
			tileMap.Build();
		}
	}
}

[ExecuteInEditMode]
[RequireComponent(
	typeof(SpriteRenderer),
	typeof(Rigidbody2D),
	typeof(BoxCollider2D)
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

	new private BoxCollider2D collider2D;

	[SerializeField]
	private Tile[,] tiles;

	public Vector2 TileMapOrigin { get { return new Vector2(width / 2f, height / 2f); } }

	private void Awake()
	{
		SetupGameObject();
	}

	public void SetupGameObject()
	{
		Debug.Assert(tilesetTexture);
		Debug.Assert(tilesetTiles.Length > 0);

		gameObject.isStatic = true;

		spriteRenderer = GetComponent<SpriteRenderer>();

		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.isKinematic = true;

		collider2D = GetComponent<BoxCollider2D>();

		transform.position = TileMapOrigin;

		// TODO: externalize
		FindObjectOfType<Character>().transform.position = transform.position;
		Camera camera = FindObjectOfType<Camera>();
		camera.transform.position = Vector3.back * 10f + transform.position;
	}

	private void Start()
	{
		Build();
	}

	public void Build()
	{
		BuildMap();
		// TODO: generate colliders
		BuildTexture();
		collider2D.size = new Vector2(width, height);
		collider2D.enabled = false;
	}

	#region Build Map

	private void BuildMap()
	{
		tiles = new Tile[width, height];

		FillTiles(TileType.Water);

		BuildRooms();

		BuildCorridors();

		BuildWalls();
	}

	public class Room
	{
		public int left;
		public int top;
		public int width;
		public int height;

		public bool isConnected = false;

		public int right
		{
			get { return left + width - 1; }
		}

		public int bottom
		{
			get { return top + height - 1; }
		}

		public int centerX
		{
			get { return left + width / 2; }
		}

		public int centerY
		{
			get { return top + height / 2; }
		}

		public bool CollidesWith(Room other)
		{
			return !(left > other.right - 1 || top > other.bottom - 1 || right < other.left + 1 || bottom < other.top + 1);
		}
	}

	private List<Room> rooms;

	private void FillTiles(TileType type)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				tiles[i, j] = new Tile(type);
			}
		}
	}

	private void BuildRooms()
	{
		rooms = new List<Room>();

		int attemptsLeft = 10;

		while (rooms.Count < 10 && attemptsLeft > 0)
		{
			int roomWidth = Random.Range(4, 14);
			int roomHeight = Random.Range(4, 10);

			Room room = new Room();
			room.left = Random.Range(0, width - roomWidth);
			room.top = Random.Range(0, height - roomHeight);
			room.width = roomWidth;
			room.height = roomHeight;

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
		for (int x = 0; x < room.width; x++)
		{
			for (int y = 0; y < room.height; y++)
			{
				if (x == 0 || x == room.width - 1 || y == 0 || y == room.height - 1)
				{
					tiles[room.left + x, room.top + y] = new Tile(TileType.Wall);
				}
				else
				{
					tiles[room.left + x, room.top + y] = new Tile(TileType.Floor);
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
				int j = Random.Range(1, rooms.Count);
				BuildCorridor(rooms[i], rooms[(i + j) % rooms.Count]);
			}
		}
	}

	private void BuildCorridor(Room sourceRoom, Room targetRoom)
	{
		int x = sourceRoom.centerX;
		int y = sourceRoom.centerY;

		while (x != targetRoom.centerX)
		{
			tiles[x, y] = new Tile(TileType.Floor);

			x += x < targetRoom.centerX ? 1 : -1;
		}

		while (y != targetRoom.centerY)
		{
			tiles[x, y] = new Tile(TileType.Floor);

			y += y < targetRoom.centerY ? 1 : -1;
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

	private bool HasAdjacentFloor(int x, int y)
	{
		if (x > 0 && tiles[x - 1, y].Type == TileType.Floor)
			return true;
		if (x < width - 1 && tiles[x + 1, y].Type == TileType.Floor)
			return true;
		if (y > 0 && tiles[x, y - 1].Type == TileType.Floor)
			return true;
		if (y < height - 1 && tiles[x, y + 1].Type == TileType.Floor)
			return true;

		if (x > 0 && y > 0 && tiles[x - 1, y - 1].Type == TileType.Floor)
			return true;
		if (x < width - 1 && y > 0 && tiles[x + 1, y - 1].Type == TileType.Floor)
			return true;

		if (x > 0 && y < height - 1 && tiles[x - 1, y + 1].Type == TileType.Floor)
			return true;
		if (x < width - 1 && y < height - 1 && tiles[x + 1, y + 1].Type == TileType.Floor)
			return true;

		return false;
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

	private int GetRandomTilesetTile()
	{
		int tilesPerRow = tilesetTexture.width / tileResolution;
		int rows = tilesetTexture.height / tileResolution;

		return Random.Range(0, tilesPerRow * rows);
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