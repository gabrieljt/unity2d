using System;

namespace Level
{
	[Serializable]
	public class LevelInstanceParameters
	{
		public int width;
		public int height;
		public Tile[,] tiles;
		public LevelInstance.Room[] rooms;
	}
}