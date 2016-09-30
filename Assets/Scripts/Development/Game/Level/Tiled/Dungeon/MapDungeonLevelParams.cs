using Game.Actor;
using System;

namespace Game.Level.Tiled
{
	[Serializable]
	public class MapDungeonLevelParams : ALevelParams
	{
		public MapDungeonLevelParams(int level)
			: base(level)
		{
		}

		public void SetSize(ref Map map)
		{
			map.width = map.height = Level + 9;
		}

		public void SetActors(ref MapDungeonActorSpawner mapDungeonActorSpawner, MapDungeon mapDungeon)
		{
			mapDungeonActorSpawner.actorSpawnersData = new ActorSpawnerData[3];
			mapDungeonActorSpawner.actorSpawnersData[0] = new ActorSpawnerData(ActorType.Player, 1);
			mapDungeonActorSpawner.actorSpawnersData[1] = new ActorSpawnerData(ActorType.Exit, 1);

			var availableTiles = mapDungeonActorSpawner.MapDungeon.Map.GetTilesOfTypeWithIndex(TileType.Floor).Count;
			mapDungeonActorSpawner.actorSpawnersData[2] = new ActorSpawnerData(ActorType.Slime, UnityEngine.Random.Range(0, availableTiles - 2) / mapDungeon.Rooms.Length);
		}
	}
}