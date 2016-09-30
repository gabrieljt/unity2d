using Game.Level;
using System;
using UnityEngine;

namespace Dungeon.Game.Level
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(DungeonLevel))]
	public class DungeonLevelInspector : ALevelComponentInspector
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
				var level = (DungeonLevel)target;
				level.Load(level.Params);
			}
		}

		private void DisposeAllButton()
		{
			if (GUILayout.Button("Dispose All"))
			{
				var level = (DungeonLevel)target;
				level.DisposeAll();
			}
		}

		private void DestroyButton()
		{
			if (GUILayout.Button("Destroy"))
			{
				var level = (DungeonLevel)target;

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
		typeof(DungeonLevelBuilder)
	)]
	public class DungeonLevel : ALevel
	{
		[SerializeField]
		private DungeonLevelParams @params;

		public DungeonLevelParams Params { get { return @params; } }

		[SerializeField]
		private DungeonLevelBuilder builder;

		private void Awake()
		{
			builder = GetComponent<DungeonLevelBuilder>();
		}

		public override void Load(ALevelParams @params)
		{
			Debug.Assert(state == LevelState.Unloaded);
			if (state == LevelState.Unloaded)
			{
				state = LevelState.Unbuilt;
				this.@params = @params as DungeonLevelParams;
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

		public void DisposeAll()
		{
			builder.Dungeon.Built -= OnDungeonBuilt;
			builder.Built -= OnBuilderBuilt;
			builder.DisposeAll();
			Dispose();
		}
	}
}