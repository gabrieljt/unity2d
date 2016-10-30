using System;
using UnityEngine;

[RequireComponent(
    typeof(LevelBuilder)
)]
public class Level : ALevelComponent
{
    public LevelParams @params;

    [SerializeField]
    private LevelBuilder levelBuilder;

    private void Awake()
    {
        levelBuilder = GetComponent<LevelBuilder>();
    }

    private void Start()
    {
        Build();
    }

    public override void Build()
    {
        levelBuilder.GetComponent<MapDungeon>().Built += OnDungeonBuilt;
        levelBuilder.Built += OnBuilderBuilt;

        var map = levelBuilder.GetComponent<Map>();
        @params.SetSize(ref map);
        levelBuilder.Build();
    }

    private void OnDungeonBuilt(Type type)
    {
        var actorSpawners = levelBuilder.GetComponent<MapActorSpawners>();
        @params.SetActorSpawnersData(ref actorSpawners, levelBuilder.GetComponent<Map>(), levelBuilder.GetComponent<MapDungeon>());
    }

    private void OnBuilderBuilt(Type type)
    {
        Built(GetType());
    }

    public override void Dispose()
    {
        levelBuilder.GetComponent<MapDungeon>().Built -= OnDungeonBuilt;
        levelBuilder.Built -= OnBuilderBuilt;
    }
}