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

			if (GUILayout.Button("Load"))
			{
				var mapDungeonLevel = (MapDungeonLevel)target;
				mapDungeonLevel.Load(mapDungeonLevel.MapDungeonLevelParams.Level);
			}

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

			if (GUILayout.Button("Destroy"))
			{
				var mapDungeonLevel = (MapDungeonLevel)target;
				Destroy(mapDungeonLevel.gameObject);
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(MapDungeonLevelBuilder)
	)]
	public class MapDungeonLevel : ALevel
	{
		[SerializeField]
		private MapDungeonLevelParams mapDungeonLevelParams;

		public MapDungeonLevelParams MapDungeonLevelParams { get { return mapDungeonLevelParams; } }

		[SerializeField]
		private MapDungeonLevelBuilder mapDungeonLevelBuilder;

		public MapDungeonLevelBuilder MapDungeonLevelBuilder { get { return mapDungeonLevelBuilder; } }

		private void Awake()
		{
			state = LevelState.Unbuilt;

			mapDungeonLevelBuilder = GetComponent<MapDungeonLevelBuilder>();
		}

		public override void Load(int level)
		{
			var mapDungeonLevelParams = new MapDungeonLevelParams(level);

			var map = mapDungeonLevelBuilder.Map;
			mapDungeonLevelParams.SetMapSize(ref map);
		}

		public override void Build()
		{
			state = LevelState.Building;
			mapDungeonLevelBuilder.Built += OnMapDungeonLevelBuilderBuilt;
			mapDungeonLevelBuilder.Build();
		}

		private void OnMapDungeonLevelBuilderBuilt()
		{
			mapDungeonLevelBuilder.Built -= OnMapDungeonLevelBuilderBuilt;
			state = LevelState.Built;

			// calculate rules from level components; e.g. maximum steps from rooms
			// get game info from level compononets; e.g. player, exit

			Built();
		}

		public override void Dispose()
		{
			state = LevelState.Unbuilt;
			mapDungeonLevelBuilder.Built = null;
		}
	}
}