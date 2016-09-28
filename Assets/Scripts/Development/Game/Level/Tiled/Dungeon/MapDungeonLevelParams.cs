using System;
using UnityEngine;

namespace Game.Level.Tiled
{
	[Serializable]
	public class MapDungeonLevelParams
	{
		[SerializeField]
		private int level;

		public int Level { get { return level; } }

		public MapDungeonLevelParams(int level)
		{
			this.level = level;
		}

		public void SetMapSize(ref Map map)
		{
			map.width = map.height = level + 9;
		}
	}
}