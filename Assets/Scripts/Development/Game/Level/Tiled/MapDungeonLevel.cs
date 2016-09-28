using System;
using UnityEngine;

namespace Game.Level.Tiled
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeonLevel))]
	public class MapDungeonLevelInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build"))
			{
				var mapDungeonLevel = (MapDungeonLevel)target;
				mapDungeonLevel.Build();
			}

			if (GUILayout.Button("Dispose"))
			{
				var mapDungeonLevel = (MapDungeonLevel)target;
				mapDungeonLevel.Dispose();
			}
		}
	}

#endif

	public enum LevelState
	{
		Unbuilt,
		Building,
		Built,
		Ready,
		InGame,
		Ended
	}

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(MapDungeonLevelBuilder)
	)]
	public class MapDungeonLevel : MonoBehaviour, IBuildable, IDestroyable, IDisposable
	{
		[SerializeField]
		private int level;

		[SerializeField]
		private LevelState state = LevelState.Unbuilt;

		[SerializeField]
		private MapDungeonLevelBuilder mapDungeonLevelBuilder;

		public MapDungeonLevelBuilder MapDungeonLevelBuilder { get { return mapDungeonLevelBuilder; } }

		private Action built = delegate { };

		public Action Built { get { return built; } set { built = value; } }

		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		private void Awake()
		{
			state = LevelState.Unbuilt;

			mapDungeonLevelBuilder = GetComponent<MapDungeonLevelBuilder>();
		}

		public void Build()
		{
			state = LevelState.Building;
			mapDungeonLevelBuilder.Built += OnMapDungeonLevelBuilderBuilt;
			mapDungeonLevelBuilder.Build();
		}

		private void OnMapDungeonLevelBuilderBuilt()
		{
			mapDungeonLevelBuilder.Built -= OnMapDungeonLevelBuilderBuilt;
			state = LevelState.Built;
		}

		public void Dispose()
		{
			state = LevelState.Unbuilt;
			mapDungeonLevelBuilder.Built = null;
		}

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}