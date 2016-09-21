using Level;
using System;

namespace Level
{
	[Serializable]
	public class LevelInstanceParameters
	{
		public int height;
		public LevelInstance.Room[] rooms;
		public Tile[,] tiles;
		public int width;
	}
}