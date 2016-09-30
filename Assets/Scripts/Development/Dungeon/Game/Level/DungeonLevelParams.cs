using Dungeon.Game.TileMap;
using Game.Actor;
using Game.Level;
using Game.TileMap;
using System;

namespace Dungeon.Game.Level
{
	[Serializable]
	public class DungeonLevelParams : ALevelParams
	{
		public DungeonLevelParams(int level)
			: base(level)
		{
		}

		public void SetSize(ref Map map)
		{
			map.width = map.height = Level + 9;
		}

		public void SetSpawnersData(ref MapActorSpawners spawners, DungeonMap dungeon)
		{
			spawners.spawnersData = new ActorSpawnerData[3];
			spawners.spawnersData[0] = new ActorSpawnerData(ActorType.Player, 1);
			spawners.spawnersData[1] = new ActorSpawnerData(ActorType.Exit, 1);

			var availableTiles = dungeon.Map.GetTilesWithMapIndexes(TileType.Floor).Count;
			spawners.spawnersData[2] = new ActorSpawnerData(ActorType.Slime,
				UnityEngine.Random.Range(0, availableTiles - 2) / dungeon.Rooms.Length);
		}
	}
}