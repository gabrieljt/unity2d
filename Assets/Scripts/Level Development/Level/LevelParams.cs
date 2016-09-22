using System;
using UnityEngine;

namespace Level
{
	[Serializable]
	public class LevelParams : IMapParams, IRoomMapParams
	{
		[SerializeField]
		private int width;

		[SerializeField]
		private int height;

		[SerializeField]
		private Tile[,] tiles;

		[SerializeField]
		private RoomMap.Room[] rooms;

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

		public RoomMap.Room[] Rooms
		{
			get
			{
				return rooms;
			}

			set
			{
				rooms = value;
			}
		}

		public LevelParams(GameObject level)
		{
			var levelMap = level.GetComponent<Map>();
			Width = levelMap.Width;
			Height = levelMap.Height;
			Tiles = levelMap.Tiles;

			var levelRoomMap = level.GetComponent<RoomMap>();
			Rooms = levelRoomMap.Rooms;
		}

		public LevelParams()
		{
		}
	}
}