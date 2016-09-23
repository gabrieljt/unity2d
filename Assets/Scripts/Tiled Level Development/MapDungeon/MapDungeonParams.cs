using UnityEngine;

namespace TiledLevel
{
	public class MapDungeonParams : IMapDungeonParams
	{
		[SerializeField]
		private MapDungeon.Room[] dungeons;

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

		public MapDungeonParams(MapDungeon dungeonMap)
		{
			Dungeons = dungeonMap.Dungeons;
		}

		public MapDungeonParams()
		{
		}
	}
}