using System;
using UnityEngine;

[RequireComponent(
    typeof(LevelBuilder)
)]
public class Level : ALevelComponent
{
    public LevelParams @params;

    [SerializeField]
    private LevelBuilder builder;

    private void Awake()
    {
        builder = GetComponent<LevelBuilder>();
    }

    private void Start()
    {
        Build();
    }

    public override void Build()
    {
        builder.GetComponent<MapDungeon>().Built += OnDungeonBuilt;
        builder.Built += OnBuilderBuilt;

        var map = builder.GetComponent<Map>();
        @params.SetSize(ref map);
        builder.Build();
    }

    private void OnDungeonBuilt(Type type)
    {
        var actorSpawners = builder.GetComponent<MapActorSpawners>();
        @params.SetActorSpawnersData(ref actorSpawners, builder.GetComponent<Map>(), builder.GetComponent<MapDungeon>());
    }

    private void OnBuilderBuilt(Type type)
    {
        Built(GetType());
    }

    public override void Dispose()
    {
        builder.GetComponent<MapDungeon>().Built -= OnDungeonBuilt;
        builder.Built -= OnBuilderBuilt;
    }
}