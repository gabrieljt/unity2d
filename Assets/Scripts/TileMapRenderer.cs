using UnityEditor;
using UnityEngine;



[ExecuteInEditMode]
public abstract class TileMapRenderer : MonoBehaviour
{
	[SerializeField]
	[Range(1, 128)]
	protected int width;

	[SerializeField]
	[Range(1, 128)]
	protected int height;

	[SerializeField]
	protected int tileResolution = 16;

	[SerializeField]
	protected Texture2D tilesTexture;

	protected virtual void Awake()
	{
		Debug.Assert(tilesTexture);
	}

	private void Start()
	{
		Render();
	}

	public abstract void Render();

	protected Color[][] GetPixelsFromTexture()
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

	protected int GetRandomTileTextureIndex()
	{
		int tilesPerRow = tilesTexture.width / tileResolution;
		int rows = tilesTexture.height / tileResolution;

		return Random.Range(0, tilesPerRow * rows);
	}

	protected Texture2D BuildTexture()
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

		return texture;
	}
}