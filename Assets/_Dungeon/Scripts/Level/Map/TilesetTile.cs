using System;
using UnityEngine;

[Serializable]
public class TilesetTile
{
    [SerializeField]
    private TileType type;

    public TileType Type { get { return type; } }

    [SerializeField]
    private int index;

    public int Index { get { return index; } }
}