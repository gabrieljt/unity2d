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

	private void BuildMap()
	{
		tiles = new Tile[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				tiles[i, j] = i == j ? new Tile(TileType.Water) : new Tile(TileType.Floor);
			}
		}
	}

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