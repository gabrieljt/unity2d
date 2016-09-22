using System;
using UnityEngine;

namespace TiledLevel
{
	[Serializable]
	public class MapParams : IMapParams
	{
		[SerializeField]
		private int width;

		[SerializeField]
		private int height;

		[SerializeField]
		private Tile[,] tiles;

		public int Width
		{
			get
			{
				return width;
			}

			set
			{
				width = value;
			}
		}

		public int Height
		{
			get
			{
				return height;
			}

			set
			{
				height = value;
			}
		}

		public Tile[,] Tiles
		{
			get
			{
				return tiles;
			}

			set
			{
				tiles = value;
			}
		}

		public MapParams(Map map)
		{
			Width = map.Width;
			Height = map.Height;
			Tiles = map.Tiles;
		}

		public MapParams()
		{
		}
	}
}