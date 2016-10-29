using System;
using UnityEngine;

[Serializable]
public class GameParams
{
    [SerializeField]
    [Range(1, 100)]
    private int level = 1;

    public int Level { get { return level; } }

    public LevelParams levelParams;

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

    public GameParams(int level)
    {
        this.level = level;
        this.levelParams = new LevelParams(level);
    }

    public void StepTaken()
    {
        ++stepsTaken;
        ++totalStepsTaken;
    }

    public void SetMaximumSteps(Map map, MapDungeon dungeon, MapActorSpawners spawners)
    {
        maximumSteps = stepsTaken = 0;
        foreach (var room in dungeon.Rooms)
        {
            maximumSteps += (int)Vector2.Distance(room.Center, map.Center);
        }

        maximumSteps = (maximumSteps / (level * dungeon.Rooms.Length) + spawners.actorsContainers[ActorType.Slime].Count * 3 + 10) * 2;
    }
}