using System;
using UnityEngine;

namespace TiledLevel
{
	[Serializable]
	public class TilesetTile
	{
		[SerializeField]
		private TileType type;

		public TileType Type { get { return type; } }

		[SerializeField]
		private int tilesetIndex;

		public int TilesetIndex { get { return tilesetIndex; } }
	}
}