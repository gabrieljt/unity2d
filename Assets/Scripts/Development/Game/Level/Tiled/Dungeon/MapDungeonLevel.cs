using UnityEngine;

namespace Game.Level.Tiled
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeonLevel))]
	public class MapDungeonLevelInspector : ALevelComponentInspector
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			LoadButton();
			BuildButton();
			DisposeButton();
			DestroyButton();
		}

		private void LoadButton()
		{
			if (GUILayout.Button("Load"))
			{
				var mapDungeonLevel = (MapDungeonLevel)target;
				mapDungeonLevel.Load(mapDungeonLevel.MapDungeonLevelParams.Level);
			}
		}

		private void DestroyButton()
		{
			if (GUILayout.Button("Destroy"))
			{
				var mapDungeonLevel = (MapDungeonLevel)target;

				if (Application.isPlaying)
				{
					Destroy(mapDungeonLevel.gameObject);
				}
				else
				{
					DestroyImmediate(mapDungeonLevel.gameObject);
				}
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
			mapDungeonLevelBuilder = GetComponent<MapDungeonLevelBuilder>();
		}

		public override void Load(int level)
		{
			Debug.Assert(state == LevelState.Unloaded);
			if (state == LevelState.Unloaded)
			{
				state = LevelState.Unbuilt;
				var mapDungeonLevelParams = new MapDungeonLevelParams(level);

				var map = mapDungeonLevelBuilder.Map;
				mapDungeonLevelParams.SetMapSize(ref map);
			}
		}

		public override void Build()
		{
			Debug.Assert(state == LevelState.Unbuilt);
			if (state == LevelState.Unbuilt)
			{
				state = LevelState.Building;
				mapDungeonLevelBuilder.Built += OnMapDungeonLevelBuilderBuilt;
				mapDungeonLevelBuilder.Build();
			}
		}

		private void OnMapDungeonLevelBuilderBuilt()
		{
			state = LevelState.Built;
			mapDungeonLevelBuilder.Built -= OnMapDungeonLevelBuilderBuilt;

			// calculate rules from level components; e.g. maximum steps from rooms
			// get game info from level compononets; e.g. player, exit

			Built();
		}

		public override void Dispose()
		{
			state = LevelState.Unloaded;
			mapDungeonLevelParams = null;
			mapDungeonLevelBuilder.Dispose();
		}
	}
}