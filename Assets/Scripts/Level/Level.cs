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
		DestroyButton();
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

	private void DestroyButton()
	{
		if (GUILayout.Button("Destroy"))
		{
			var level = (Level)target;

			if (Application.isPlaying)
			{
				Destroy(level.gameObject);
			}
			else
			{
				DestroyImmediate(level.gameObject);
			}
		}
	}
}

#endif

[ExecuteInEditMode]
[RequireComponent(
	typeof(LevelBuilder)
)]
public class Level : ALevel
{
	[SerializeField]
	private LevelParams @params;

	public LevelParams Params { get { return @params; } }

	[SerializeField]
	private LevelBuilder builder;

	private void Awake()
	{
		builder = GetComponent<LevelBuilder>();
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
			builder.Dungeon.Built += OnDungeonBuilt;
			builder.Built += OnBuilderBuilt;

			var map = builder.Map;
			@params.SetSize(ref map);
			builder.Build();
		}
	}

	private void OnDungeonBuilt(Type type)
	{
		builder.Dungeon.Built -= OnDungeonBuilt;

		var spawnersMap = builder.Spawners;
		@params.SetSpawnersData(ref spawnersMap, builder.Dungeon);
	}

	private void OnBuilderBuilt(Type type)
	{
		state = LevelState.Built;
		builder.Built -= OnBuilderBuilt;

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
			builder.Dungeon.Built -= OnDungeonBuilt;
			builder.Built -= OnBuilderBuilt;
			builder.Dispose(true);
		}
		Dispose();
	}
}