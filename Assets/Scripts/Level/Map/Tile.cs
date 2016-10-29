using System;
using UnityEngine;

public enum TileType
{
    None,
    Water,
    Floor,
    Wall,
}

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