using Game.Level.Tiled;
using System;
using UnityEngine;

namespace Game
{
	[Serializable]
	public class MapDungeonGameParams
	{
		[SerializeField]
		[Range(1, 100)]
		private int level = 1;

		public int Level { get { return level; } }

		[SerializeField]
		private MapDungeonLevelParams mapDungeonLevelParams;

		public MapDungeonLevelParams MapDungeonLevelParams { get { return mapDungeonLevelParams; } }

		[SerializeField]
		private int stepsTaken = 0;

		public int StepsTaken { get { return stepsTaken; } }

		[SerializeField]
		private static int totalStepsTaken = 0;

		public static int TotalStepsTaken { get { return totalStepsTaken; } }

		[SerializeField]
		private int maximumSteps;

		public int MaximumSteps { get { return maximumSteps; } }

		public int StepsLeft { get { return maximumSteps - stepsTaken; } }

		public MapDungeonGameParams(int level)
		{
			this.level = level;
			this.mapDungeonLevelParams = new MapDungeonLevelParams(level);
		}

		public void StepTaken()
		{
			++stepsTaken;
			++totalStepsTaken;
		}

		public void SetMaximumSteps(MapDungeon mapDungeon, MapDungeonActorSpawner mapDungeonActorSpawner, Vector2 tileMapOrigin)
		{
			maximumSteps = stepsTaken = 0;
			foreach (var room in mapDungeon.Rooms)
			{
				maximumSteps += (int)Vector2.Distance(room.Center, tileMapOrigin);
			}

			maximumSteps = maximumSteps / (level * mapDungeon.Rooms.Length) + mapDungeonActorSpawner.spawnedActors[Actor.ActorType.Slime].Count * 3 + 10;
		}
	}
}