using UnityEngine;

namespace Game.Level.Tiled
{
	using System;

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
				mapDungeonLevel.Load(mapDungeonLevel.MapDungeonLevelParams);
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

		private void Awake()
		{
			mapDungeonLevelBuilder = GetComponent<MapDungeonLevelBuilder>();
		}

		public override void Load(ALevelParams mapDungeonLevelParams)
		{
			Debug.Assert(state == LevelState.Unloaded);
			if (state == LevelState.Unloaded)
			{
				state = LevelState.Unbuilt;
				this.mapDungeonLevelParams = mapDungeonLevelParams as MapDungeonLevelParams;
			}
		}

		public override void Build()
		{
			Debug.Assert(state == LevelState.Unbuilt);
			if (state == LevelState.Unbuilt)
			{
				state = LevelState.Building;
				mapDungeonLevelBuilder.MapDungeon.Built += OnMapDungeonBuilt;
				mapDungeonLevelBuilder.Built += OnMapDungeonLevelBuilderBuilt;

				var map = mapDungeonLevelBuilder.Map;
				mapDungeonLevelParams.SetSize(ref map);
				mapDungeonLevelBuilder.Build();
			}
		}

		private void OnMapDungeonBuilt(Type levelComponentBuiltType)
		{
			mapDungeonLevelBuilder.MapDungeon.Built -= OnMapDungeonBuilt;

			var mapDungeonActorSpawner = mapDungeonLevelBuilder.MapDungeonActorSpawner;
			mapDungeonLevelParams.SetActors(ref mapDungeonActorSpawner);
		}

		private void OnMapDungeonLevelBuilderBuilt(Type levelComponentBuiltType)
		{
			state = LevelState.Built;
			mapDungeonLevelBuilder.Built -= OnMapDungeonLevelBuilderBuilt;

			Built(GetType());
		}

		public override void Dispose()
		{
			state = LevelState.Unloaded;
			mapDungeonLevelBuilder.Dispose();
		}
	}
}