using UnityEngine;

namespace Game.Level.Tiled
{
	public enum MapTilesetType
	{
		Dungeon = 0,
	}

	[CreateAssetMenu(fileName = "NewMapTileset", menuName = "Map/Tileset", order = 1)]
	public class MapTileset : ScriptableObject
	{
		[SerializeField]
		private MapTilesetType type = MapTilesetType.Dungeon;

		public MapTilesetType Type { get { return type; } }

		[SerializeField]
		private const int tileResolution = 16;

		[SerializeField]
		private Texture2D tilesetTexture;

		public Texture2D TilesetTexture { get { return tilesetTexture; } }

		[SerializeField]
		private TilesetTile[] tilesetTiles;

		public TilesetTile[] TilesetTiles { get { return tilesetTiles; } }

		private static Color[][] GetPixelsFromTexture(Texture2D tilesetTexture, int tileResolution = 16)
		{
			var tilesPerRow = tilesetTexture.width / tileResolution;
			var rows = tilesetTexture.height / tileResolution;
			var tilesPixels = new Color[tilesPerRow * rows][];

			for (int y = 0; y < rows; y++)
			{
				for (int x = 0; x < tilesPerRow; x++)
				{
					tilesPixels[y * tilesPerRow + x] = tilesetTexture.GetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution);
				}
			}

			return tilesPixels;
		}

		public static int GetTilesetTileIndexByType(TilesetTile[] tilesetTiles, TileType type)
		{
			return System.Array.Find(tilesetTiles, tilesetTile => tilesetTile.Type == type).TilesetIndex;
		}

		public static Texture2D BuildTexture(Map map, Texture2D tilesetTexture, TilesetTile[] tilesetTiles)
		{
			Debug.Assert(tilesetTexture);
			Debug.Assert(tilesetTiles.Length > 0);

			var textureWidth = map.Width * tileResolution;
			var textureHeight = map.Height * tileResolution;
			var texture = new Texture2D(textureWidth, textureHeight);
			var tilesPixels = GetPixelsFromTexture(tilesetTexture, tileResolution);

			for (int y = 0; y < map.Height; y++)
			{
				for (int x = 0; x < map.Width; x++)
				{
					Color[] pixels = tilesPixels[GetTilesetTileIndexByType(tilesetTiles, map.Tiles[x, y].Type)];
					texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, pixels);
				}
			}

			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.Apply();

			return texture;
		}
	}
}