using UnityEngine;

public enum TilesetType
{
	Dungeon = 0,
}

[CreateAssetMenu(fileName = "NewMapTileset", menuName = "Map/Tileset", order = 1)]
public class Tileset : ScriptableObject
{
	[SerializeField]
	private TilesetType type = TilesetType.Dungeon;

	public TilesetType Type { get { return type; } }

	[SerializeField]
	private const int tileResolution = 16;

	[SerializeField]
	private Texture2D texture;

	public Texture2D Texture { get { return texture; } }

	[SerializeField]
	private TilesetTile[] tilesetTiles;

	public TilesetTile[] TilesetTiles { get { return tilesetTiles; } }

	private static Color[][] GetPixelsFromTexture(Texture2D texture, int tileResolution = 16)
	{
		var tilesPerRow = texture.width / tileResolution;
		var rows = texture.height / tileResolution;
		var pixels = new Color[tilesPerRow * rows][];

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < tilesPerRow; x++)
			{
				pixels[y * tilesPerRow + x] = texture.GetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution);
			}
		}

		return pixels;
	}

	public static int GetTilesetTileIndexByType(TilesetTile[] tilesetTiles, TileType type)
	{
		return System.Array.Find(tilesetTiles, tilesetTile => tilesetTile.Type == type).Index;
	}

	public static Texture2D BuildTexture(Map map, Texture2D tilesetTexture, TilesetTile[] tilesetTiles)
	{
		Debug.Assert(tilesetTexture);
		Debug.Assert(tilesetTiles.Length > 0);

		var textureWidth = map.width * tileResolution;
		var textureHeight = map.height * tileResolution;
		var texture = new Texture2D(textureWidth, textureHeight);
		var tilesetPixels = GetPixelsFromTexture(tilesetTexture, tileResolution);

		for (int y = 0; y < map.height; y++)
		{
			for (int x = 0; x < map.width; x++)
			{
				Color[] pixels = tilesetPixels[GetTilesetTileIndexByType(tilesetTiles, map.tiles[x, y].Type)];
				texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, pixels);
			}
		}

		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.Apply();

		return texture;
	}
}