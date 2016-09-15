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
	private Texture2D tilesTexture;

	private SpriteRenderer spriteRenderer;

	new private Rigidbody2D rigidbody2D;

	new private BoxCollider2D collider2D;

	private void Awake()
	{
		SetupGameObject();
	}

	public void SetupGameObject()
	{
		Debug.Assert(tilesTexture);

		gameObject.isStatic = true;

		spriteRenderer = GetComponent<SpriteRenderer>();

		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0f;
		rigidbody2D.isKinematic = true;

		collider2D = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		Build();
	}

	public void Build()
	{
		// TODO: build logical map
		BuildTexture();
		// TODO: generate colliders
		collider2D.size = new Vector2(width, height);
	}

	private Color[][] GetPixelsFromTexture()
	{
		int tilesPerRow = tilesTexture.width / tileResolution;
		int rows = tilesTexture.height / tileResolution;

		Color[][] tilesPixels = new Color[tilesPerRow * rows][];

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < tilesPerRow; x++)
			{
				tilesPixels[y * tilesPerRow + x] = tilesTexture.GetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution);
			}
		}

		return tilesPixels;
	}

	private int GetRandomTileTextureIndex()
	{
		int tilesPerRow = tilesTexture.width / tileResolution;
		int rows = tilesTexture.height / tileResolution;

		return Random.Range(0, tilesPerRow * rows);
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
				Color[] pixels = tilesPixels[GetRandomTileTextureIndex()]; // TODO: read tile type from logical map
				texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, pixels);
			}
		}

		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.Apply();

		spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f, tileResolution);
	}
}