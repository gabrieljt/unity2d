using Dungeon.Game.Level;
using Dungeon.Game.TileMap;
using Game.Actor;
using Game.TileMap;
using System;
using UnityEngine;

namespace Dungeon.Game
{
	[Serializable]
	public class DungeonGameParams
	{
		[SerializeField]
		[Range(1, 100)]
		private int level = 1;

		public int Level { get { return level; } }

		public DungeonLevelParams levelParams;

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

		public DungeonGameParams(int level)
		{
			this.level = level;
			this.levelParams = new DungeonLevelParams(level);
		}

		public void StepTaken()
		{
			++stepsTaken;
			++totalStepsTaken;
		}

		public void SetMaximumSteps(DungeonMap dungeon, MapActorSpawners spawners)
		{
			maximumSteps = stepsTaken = 0;
			foreach (var room in dungeon.Rooms)
			{
				maximumSteps += (int)Vector2.Distance(room.Center, dungeon.Map.Center);
			}

			maximumSteps = maximumSteps / (level * dungeon.Rooms.Length) + spawners.actorsContainers[ActorType.Slime].Count * 3 + 10;
		}
	}
}