using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Tiled
{
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
		private Queue<IBuildable> buildableComponentsQueue = new Queue<IBuildable>();

		private IBuildable[] buildableComponentsArray = new IBuildable[0];

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		[SerializeField]
		private MapDungeon mapDungeon;

		private MapDungeon MapDungeon { get { return mapDungeon; } }

		[SerializeField]
		private MapDungeonActorSpawner mapDungeonActorSpawner;

		private MapDungeonActorSpawner MapDungeonActorSpawner { get { return mapDungeonActorSpawner; } }

		[SerializeField]
		private MapCollision mapCollision;

		public MapCollision MapCollision { get { return mapCollision; } }

		[SerializeField]
		private MapRenderer mapRenderer;

		public MapRenderer MapRenderer { get { return mapRenderer; } }

		private void Awake()
		{
			SetBuildableComponentsArray();
		}

		private void Update()
		{
			if (buildableComponentsQueue.Count > 0)
			{
				buildableComponentsQueue.Dequeue().Build();
				if (buildableComponentsQueue.Count == 0)
				{
					Built();
				}
			}
		}

		private void SetBuildableComponentsArray()
		{
			buildableComponentsArray = new IBuildable[5];
			buildableComponentsArray[0] = map = GetComponent<Map>();
			buildableComponentsArray[1] = mapDungeon = GetComponent<MapDungeon>();
			buildableComponentsArray[2] = mapDungeonActorSpawner = GetComponent<MapDungeonActorSpawner>();
			buildableComponentsArray[3] = mapCollision = GetComponent<MapCollision>();
			buildableComponentsArray[4] = mapRenderer = GetComponent<MapRenderer>();
		}

		private void SetBuildableComponentsQueue()
		{
			Debug.Assert(buildableComponentsQueue.Count == 0);
			foreach (var levelComponent in buildableComponentsArray)
			{
				buildableComponentsQueue.Enqueue(levelComponent);
			}
		}

		public override void Build()
		{
			SetBuildableComponentsQueue();
		}

		public override void Dispose()
		{
			buildableComponentsQueue.Clear();
		}
	}
}