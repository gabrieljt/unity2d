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

			if (GUILayout.Button("Build Level"))
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

	/// <summary>
	/// TODO: externalize texture handling
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(
		typeof(SpriteRenderer)
	)]
	public class Map : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer spriteRenderer;

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

		[SerializeField]
		private int tileResolution = 16;

		[SerializeField]
		private Texture2D tilesetTexture;

		[SerializeField]
		private TilesetTile[] tilesetTiles;

		public Action<IMapParams> Built = delegate { };

		private void Awake()
		{
			Debug.Assert(tilesetTexture);
			Debug.Assert(tilesetTiles.Length > 0);

			spriteRenderer = GetComponent<SpriteRenderer>();
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

		private void SetValues(IMapParams mapParams)
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

			BuildTexture();
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

		#region Build Texture

		private Color[][] GetPixelsFromTexture()
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

		public int GetTilesetTileIndexByType(TileType type)
		{
			return System.Array.Find(tilesetTiles, tilesetTile => tilesetTile.Type == type).TilesetIndex;
		}

		private void BuildTexture()
		{
			var textureWidth = width * tileResolution;
			var textureHeight = height * tileResolution;
			var texture = new Texture2D(textureWidth, textureHeight);

			var tilesPixels = GetPixelsFromTexture();

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
}