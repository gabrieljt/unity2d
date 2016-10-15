using System;
using UnityEngine;

[Serializable]
public class LevelParams
{
	[SerializeField]
	private int level = 1;

	public int Level { get { return level; } }

	public LevelParams(int level)
	{
		this.level = level;
	}

	public void SetSize(ref Map map)
	{
		map.width = map.height = level + 9;
	}

	public void SetSpawnersData(ref ActorSpawners spawners, Dungeon dungeon)
	{
		spawners.spawnersData = new ActorSpawnerData[3];
		spawners.spawnersData[0] = new ActorSpawnerData(ActorType.Player, 1);
		spawners.spawnersData[1] = new ActorSpawnerData(ActorType.Exit, 1);

		var availableTiles = dungeon.Map.GetTilesWithMapIndexes(TileType.Floor).Count;
		spawners.spawnersData[2] = new ActorSpawnerData(ActorType.Slime,
			UnityEngine.Random.Range(0, availableTiles - 2) / dungeon.Rooms.Length);
	}
}