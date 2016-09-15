using System;
using UnityEngine;

[Serializable]
public class Tile
{
	[SerializeField]
	private TileType type;

	public TileType Type { get { return type; } }

	public Tile(TileType type)
	{
		this.type = type;
	}
}