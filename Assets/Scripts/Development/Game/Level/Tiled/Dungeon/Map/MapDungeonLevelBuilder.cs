using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Tiled
{
	using System;

#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeonLevelBuilder))]
	public class MapDungeonLevelBuilderInspector : ALevelComponentInspector
	{
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(Map),
		typeof(MapDungeon),
		typeof(MapDungeonActorSpawner)
	)]
	[RequireComponent(
		typeof(MapCollision),
		typeof(MapRenderer)
	)]
	public class MapDungeonLevelBuilder : ALevelComponent
	{
		private Queue<ALevelComponent> levelComponentsBuildQueue = new Queue<ALevelComponent>();

		private int levelComponentsBuilt;

		[SerializeField]
		private ALevelComponent[] levelComponents = new ALevelComponent[0];

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		[SerializeField]
		private MapDungeon mapDungeon;

		public MapDungeon MapDungeon { get { return mapDungeon; } }

		[SerializeField]
		private MapDungeonActorSpawner mapDungeonActorSpawner;

		public MapDungeonActorSpawner MapDungeonActorSpawner { get { return mapDungeonActorSpawner; } }

		[SerializeField]
		private MapCollision mapCollision;

		public MapCollision MapCollision { get { return mapCollision; } }

		[SerializeField]
		private MapRenderer mapRenderer;

		public MapRenderer MapRenderer { get { return mapRenderer; } }

		public override void Build()
		{
			Debug.Assert(levelComponentsBuilt == 0);
			if (levelComponentsBuilt == 0)
			{
				SetLevelComponents();
#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					SetLevelComponentsBuildQueue();
				}
				else
				{
					BuildImmediate();
				}
#else
				SetLevelComponentsBuildQueue();
#endif
			}
		}

		private void SetLevelComponents()
		{
			levelComponents = new ALevelComponent[5];
			levelComponents[0] = map = GetComponent<Map>();
			levelComponents[1] = mapDungeon = GetComponent<MapDungeon>();
			levelComponents[2] = mapDungeonActorSpawner = GetComponent<MapDungeonActorSpawner>();
			levelComponents[3] = mapCollision = GetComponent<MapCollision>();
			levelComponents[4] = mapRenderer = GetComponent<MapRenderer>();

			foreach (var levelComponent in levelComponents)
			{
				levelComponent.Built += OnLevelComponentBuilt;
			}
		}

		private void OnLevelComponentBuilt()
		{
			levelComponents[levelComponentsBuilt].Built -= OnLevelComponentBuilt;

			++levelComponentsBuilt;

			if (levelComponentsBuilt == levelComponents.Length)
			{
				Built();
			}
		}

		private void SetLevelComponentsBuildQueue()
		{
			foreach (var levelComponent in levelComponents)
			{
				levelComponentsBuildQueue.Enqueue(levelComponent);
			}
		}

		private void Update()
		{
			if (levelComponentsBuildQueue.Count > 0)
			{
				levelComponentsBuildQueue.Dequeue().Build();
			}
		}

		private void BuildImmediate()
		{
			foreach (var levelComponent in levelComponents)
			{
				levelComponent.Build();
			}
		}

		public override void Dispose()
		{
			Array.Reverse(levelComponents);
			foreach (var levelComponent in levelComponents)
			{
				levelComponent.Dispose();
			}

			levelComponents = new ALevelComponent[0];
			levelComponentsBuildQueue.Clear();
			levelComponentsBuilt = 0;
		}
	}
}