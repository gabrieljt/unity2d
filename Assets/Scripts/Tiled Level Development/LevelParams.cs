using System;
using UnityEngine;

namespace TiledLevel
{
	[Serializable]
	public class LevelParams : IMapParams, IMapDungeonParams
	{
		[SerializeField]
		private int width;

		[SerializeField]
		private int height;

		[SerializeField]
		private Tile[,] tiles;

		[SerializeField]
		private MapDungeon.Room[] dungeons;

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

		public MapDungeon.Room[] Dungeons
		{
			get
			{
				return dungeons;
			}

			set
			{
				dungeons = value;
			}
		}

		public LevelParams(GameObject level)
		{
			var levelMap = level.GetComponent<Map>();
			Width = levelMap.Width;
			Height = levelMap.Height;
			Tiles = levelMap.Tiles;

			var levelDungeonMap = level.GetComponent<MapDungeon>();
			Dungeons = levelDungeonMap.Dungeons;
		}

		public LevelParams()
		{
		}
	}
}