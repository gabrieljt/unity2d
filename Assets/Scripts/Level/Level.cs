using System;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelInspector : ALevelComponentInspector
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		LoadButton();
		BuildButton();
		DisposeAllButton();
		DisposeButton();
	}

	private void LoadButton()
	{
		if (GUILayout.Button("Load"))
		{
			var level = (Level)target;
			level.Load(level.Params);
		}
	}

	private void DisposeAllButton()
	{
		if (GUILayout.Button("Dispose All"))
		{
			var level = (Level)target;
			level.Dispose(true);
		}
	}
}

#endif

[RequireComponent(
	typeof(LevelBuilder)
)]
public class Level : ALevel
{
	[SerializeField]
	private LevelParams @params;

	public LevelParams Params { get { return @params; } }

	[SerializeField]
	private LevelBuilder levelBuilder;

	private void Awake()
	{
		levelBuilder = GetComponent<LevelBuilder>();
	}

	public override void Load(LevelParams @params)
	{
		Debug.Assert(state == LevelState.Unloaded);
		if (state == LevelState.Unloaded)
		{
			state = LevelState.Unbuilt;
			this.@params = @params as LevelParams;
		}
	}

	public override void Build()
	{
		Debug.Assert(state == LevelState.Unbuilt);
		if (state == LevelState.Unbuilt)
		{
			state = LevelState.Building;
			levelBuilder.GetComponent<Dungeon>().Built += OnDungeonBuilt;
			levelBuilder.Built += OnLevelBuilderBuilt;

			var map = levelBuilder.GetComponent<Map>();
			@params.SetSize(ref map);
			levelBuilder.Build();
		}
	}

	private void OnDungeonBuilt(Type type)
	{
		levelBuilder.GetComponent<Dungeon>().Built -= OnDungeonBuilt;

		var actorSpawners = levelBuilder.GetComponent<ActorSpawners>();
		@params.SetActorSpawnersData(ref actorSpawners, levelBuilder.GetComponent<Map>(), levelBuilder.GetComponent<Dungeon>());
	}

	private void OnLevelBuilderBuilt(Type type)
	{
		state = LevelState.Built;
		levelBuilder.Built -= OnLevelBuilderBuilt;

		Built(GetType());
	}

	public override void Dispose()
	{
		state = LevelState.Unloaded;
	}

	public void Dispose(bool disposeDependencies)
	{
		if (disposeDependencies)
		{
			levelBuilder.GetComponent<Dungeon>().Built -= OnDungeonBuilt;
			levelBuilder.Built -= OnLevelBuilderBuilt;
			levelBuilder.Dispose(true);
		}
		Dispose();
	}
}