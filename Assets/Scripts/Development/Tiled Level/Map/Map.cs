using System;
using System.Collections.Generic;
using UnityEngine;

namespace TiledLevel
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(Map))]
	public class MapInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build Map"))
			{
				var map = (Map)target;

				map.Build();
			}
		}
	}

#endif

	[ExecuteInEditMode]
	public class Map : MonoBehaviour
	{
		[SerializeField]
		[Range(4, 128)]
		private int width = 16;

		public int Width { get { return width; } }

		[SerializeField]
		[Range(3, 128)]
		private int height = 9;

		public int Height { get { return height; } }

		[SerializeField]
		private Tile[,] tiles;

		public Tile[,] Tiles { get { return tiles; } }

		public Vector2 WorldPosition { get { return new Vector2(width / 2f, height / 2f); } }

		public Action Built = delegate { };

		public Action Updated = delegate { };

		public void Build(int width, int height, out Map map)
		{
			this.width = width;
			this.height = height;

			Build();

			map = this;
		}

		public void Build()
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
			var tiles = map.Tiles;
			var width = map.Width;
			var height = map.Height;

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
			var tiles = map.Tiles;
			var width = map.Width;
			var height = map.Height;
			var adjacentTilesList = new List<Tile>();

			if (x > 0 && tiles[x - 1, y].Type == type)
			{
				adjacentTilesList.Add(tiles[x - 1, y]);
			}

			if (x < width - 1 && tiles[x + 1, y].Type == type)
			{
				adjacentTilesList.Add(tiles[x + 1, y]);
			}

			if (y > 0 && tiles[x, y - 1].Type == type)
			{
				adjacentTilesList.Add(tiles[x, y - 1]);
			}

			if (y < height - 1 && tiles[x, y + 1].Type == type)
			{
				adjacentTilesList.Add(tiles[x, y + 1]);
			}

			if (x > 0 && y > 0 && tiles[x - 1, y - 1].Type == type)
			{
				adjacentTilesList.Add(tiles[x - 1, y - 1]);
			}

			if (x < width - 1 && y > 0 && tiles[x + 1, y - 1].Type == type)
			{
				adjacentTilesList.Add(tiles[x + 1, y - 1]);
			}

			if (x > 0 && y < height - 1 && tiles[x - 1, y + 1].Type == type)
			{
				adjacentTilesList.Add(tiles[x - 1, y + 1]);
			}

			if (x < width - 1 && y < height - 1 && tiles[x + 1, y + 1].Type == type)
			{
				adjacentTilesList.Add(tiles[x + 1, y + 1]);
			}

			adjacentTiles = adjacentTilesList.ToArray();
			return adjacentTiles.Length > 0;
		}
	}
}