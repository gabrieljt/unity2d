using System.Collections.Generic;
using UnityEngine;

public class Map : ALevelComponent
{
	[Range(4, 128)]
	public int width = 16;

	[Range(3, 128)]
	public int height = 9;

	public Tile[,] tiles = new Tile[0, 0];

	public Vector2 Center { get { return new Vector2(width / 2f, height / 2f); } }

	private void Awake()
	{
		gameObject.isStatic = true;
	}

	public override void Build()
	{
		SetWorldPosition();

		FillTiles(TileType.Water);

		Built(GetType());
	}

	private void SetWorldPosition()
	{
		transform.position = Center;
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

	public bool HasAdjacentFloor(int x, int y)
	{
		return HasAdjacentType(x, y, TileType.Floor);
	}

	public bool HasAdjacentType(int x, int y, TileType type)
	{
		return (x > 0 && tiles[x - 1, y].Type == type)
			|| (x < width - 1 && tiles[x + 1, y].Type == type)
			|| (y > 0 && tiles[x, y - 1].Type == type)
			|| (y < height - 1 && tiles[x, y + 1].Type == type)
			|| (x > 0 && y > 0 && tiles[x - 1, y - 1].Type == type)
			|| (x < width - 1 && y > 0 && tiles[x + 1, y - 1].Type == type)
			|| (x > 0 && y < height - 1 && tiles[x - 1, y + 1].Type == type)
			|| (x < width - 1 && y < height - 1 && tiles[x + 1, y + 1].Type == type);
	}

	public bool HasAdjacentType(int x, int y, TileType type, out Tile[] adjacentTiles)
	{
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

	public List<KeyValuePair<Tile, Vector2>> GetTilesWithMapIndexes(TileType type)
	{
		var tilesList = new List<KeyValuePair<Tile, Vector2>>();
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (tiles[x, y].Type == type)
				{
					tilesList.Add(new KeyValuePair<Tile, Vector2>(tiles[x, y], new Vector2(x, y)));
				}
			}
		}

		return tilesList;
	}

	public override void Dispose()
	{
		tiles = new Tile[0, 0];
	}
}