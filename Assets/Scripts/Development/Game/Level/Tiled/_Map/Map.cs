using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Tiled
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(Map))]
	public class MapInspector : ALevelComponentInspector
	{
	}

#endif

	[ExecuteInEditMode]
	public class Map : ALevelComponent
	{
		[Range(4, 128)]
		public int width = 16;

		[Range(3, 128)]
		public int height = 9;

		public Tile[,] tiles = new Tile[0, 0];

		public Vector2 WorldPosition { get { return new Vector2(width / 2f, height / 2f); } }

		// ALevel
		private void Awake()
		{
			gameObject.isStatic = true;
		}

		public override void Build()
		{
			SetWorldPosition();

			FillTiles(TileType.Water);

			Built();
		}

		private void SetWorldPosition()
		{
			transform.position = WorldPosition;
		}

		public void FillTiles(TileType type)
		{
			tiles = new Tile[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					tiles[x, y] = new Tile(type);
				}
			}
		}

		public static bool HasAdjacentFloor(Map map, int x, int y)
		{
			return HasAdjacentType(map, x, y, TileType.Floor);
		}

		public static bool HasAdjacentType(Map map, int x, int y, TileType type)
		{
			var tiles = map.tiles;
			var width = map.width;
			var height = map.height;

			return (x > 0 && tiles[x - 1, y].Type == type)
				|| (x < width - 1 && tiles[x + 1, y].Type == type)
				|| (y > 0 && tiles[x, y - 1].Type == type)
				|| (y < height - 1 && tiles[x, y + 1].Type == type)
				|| (x > 0 && y > 0 && tiles[x - 1, y - 1].Type == type)
				|| (x < width - 1 && y > 0 && tiles[x + 1, y - 1].Type == type)
				|| (x > 0 && y < height - 1 && tiles[x - 1, y + 1].Type == type)
				|| (x < width - 1 && y < height - 1 && tiles[x + 1, y + 1].Type == type);
		}

		public static bool HasAdjacentType(Map map, int x, int y, TileType type, out Tile[] adjacentTiles)
		{
			var tiles = map.tiles;
			var width = map.width;
			var height = map.height;
			var adjacentTilesList = new List<Tile>();

			if (x > 0 && tiles[x - 1, y].Type == type)
			{
				adjacentTilesList.Add((Tile)tiles[x - 1, y]);
			}

			if (x < width - 1 && tiles[x + 1, y].Type == type)
			{
				adjacentTilesList.Add((Tile)tiles[x + 1, y]);
			}

			if (y > 0 && tiles[x, y - 1].Type == type)
			{
				adjacentTilesList.Add((Tile)tiles[x, y - 1]);
			}

			if (y < height - 1 && tiles[x, y + 1].Type == type)
			{
				adjacentTilesList.Add((Tile)tiles[x, y + 1]);
			}

			if (x > 0 && y > 0 && tiles[x - 1, y - 1].Type == type)
			{
				adjacentTilesList.Add((Tile)tiles[x - 1, y - 1]);
			}

			if (x < width - 1 && y > 0 && tiles[x + 1, y - 1].Type == type)
			{
				adjacentTilesList.Add((Tile)tiles[x + 1, y - 1]);
			}

			if (x > 0 && y < height - 1 && tiles[x - 1, y + 1].Type == type)
			{
				adjacentTilesList.Add((Tile)tiles[x - 1, y + 1]);
			}

			if (x < width - 1 && y < height - 1 && tiles[x + 1, y + 1].Type == type)
			{
				adjacentTilesList.Add((Tile)tiles[x + 1, y + 1]);
			}

			adjacentTiles = adjacentTilesList.ToArray();
			return adjacentTiles.Length > 0;
		}

		public override void Dispose()
		{
			tiles = new Tile[0, 0];
		}
	}
}