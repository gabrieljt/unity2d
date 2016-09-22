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

			if (GUILayout.Button("Build Map"))
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

		public Action<IMapParams> Built = delegate { };

		public Action<IMapParams> Updated = delegate { };

		private void Awake()
		{
			gameObject.isStatic = true;
		}

		public void Build(ref IMapParams mapParams)
		{
			SetValues(mapParams);

			SetWorldPosition();

			FillTiles(TileType.Water);

			mapParams = new MapParams(this);

			Built(mapParams);
		}

		public void SetValues(IMapParams mapParams)
		{
			this.width = mapParams.Width;
			this.height = mapParams.Height;
			this.tiles = new Tile[width, height];
		}

		public void UpdateValues(IMapParams mapParams)
		{
			SetValues(mapParams);
			this.tiles = mapParams.Tiles;

			DisposeColliders();

			BuildColliders();

			Updated(mapParams);
		}

		private void SetWorldPosition()
		{
			transform.position = WorldPosition;
		}

		#region Build Map

		public void FillTiles(TileType type)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					tiles[x, y] = new Tile(type);
				}
			}
		}

		public void DisposeColliders()
		{
			var colliders = GetComponents<BoxCollider2D>();

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
						var collider = gameObject.AddComponent<BoxCollider2D>();
						collider.offset = new Vector2(x, y) + Vector2.one * 0.5f - WorldPosition;
						collider.size = Vector2.one;
					}
				}
			}
		}

		public static bool HasAdjacentFloor(IMapParams mapParams, int x, int y)
		{
			return HasAdjacentType(mapParams, x, y, TileType.Floor);
		}

		public static bool HasAdjacentType(IMapParams mapParams, int x, int y, TileType type)
		{
			var tiles = mapParams.Tiles;
			var width = mapParams.Width;
			var height = mapParams.Height;

			return (x > 0 && tiles[x - 1, y].Type == type)
				|| (x < width - 1 && tiles[x + 1, y].Type == type)
				|| (y > 0 && tiles[x, y - 1].Type == type)
				|| (y < height - 1 && tiles[x, y + 1].Type == type)
				|| (x > 0 && y > 0 && tiles[x - 1, y - 1].Type == type)
				|| (x < width - 1 && y > 0 && tiles[x + 1, y - 1].Type == type)
				|| (x > 0 && y < height - 1 && tiles[x - 1, y + 1].Type == type)
				|| (x < width - 1 && y < height - 1 && tiles[x + 1, y + 1].Type == type);
		}

		public static bool HasAdjacentType(IMapParams mapParams, int x, int y, TileType type, out Tile[] adjacentTiles)
		{
			var tiles = mapParams.Tiles;
			var width = mapParams.Width;
			var height = mapParams.Height;
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

		#endregion Build Map
	}
}