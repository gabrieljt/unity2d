using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Tiled
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapDungeonLevelBuilder))]
	public class MapDungeonLevelBuilderInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build"))
			{
				var mapDungeonLevelBuilder = (MapDungeonLevelBuilder)target;
				mapDungeonLevelBuilder.Build();
			}

			if (GUILayout.Button("Dispose"))
			{
				var mapDungeonLevelBuilder = (MapDungeonLevelBuilder)target;
				mapDungeonLevelBuilder.Dispose();
			}
		}
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
	public class MapDungeonLevelBuilder : MonoBehaviour, IBuildable, IDestroyable, IDisposable
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

		private Action built = delegate { };

		public Action Built { get { return built; } set { built = value; } }

		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

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

		public void Build()
		{
			SetBuildableComponentsQueue();
		}

		public void Dispose()
		{
			buildableComponentsQueue.Clear();
		}

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}