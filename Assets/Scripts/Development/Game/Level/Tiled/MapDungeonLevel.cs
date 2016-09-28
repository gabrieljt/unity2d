using System;
using System.Collections.Generic;
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
	public class MapDungeonLevel : MonoBehaviour, ILevelComponent, IDestroyable
	{
		[SerializeField]
		private int level;

		private Queue<ILevelComponent> levelComponentsBuildQueue = new Queue<ILevelComponent>();

		private ILevelComponent[] levelComponentsArray = new ILevelComponent[0];

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
			SetLevelComponentsArray();
		}

		private void Update()
		{
			if (levelComponentsBuildQueue.Count > 0)
			{
				levelComponentsBuildQueue.Dequeue().Build();
				if (levelComponentsBuildQueue.Count == 0)
				{
					Built();
				}
			}
		}

		private void SetLevelComponentsArray()
		{
			levelComponentsArray = new ILevelComponent[5];
			levelComponentsArray[0] = map = GetComponent<Map>();
			levelComponentsArray[1] = mapDungeon = GetComponent<MapDungeon>();
			levelComponentsArray[2] = mapDungeonActorSpawner = GetComponent<MapDungeonActorSpawner>();
			levelComponentsArray[3] = mapCollision = GetComponent<MapCollision>();
			levelComponentsArray[4] = mapRenderer = GetComponent<MapRenderer>();
		}

		private void SetLevelComponentsBuildQueue()
		{
			Debug.Assert(levelComponentsBuildQueue.Count == 0);
			foreach (var levelComponent in levelComponentsArray)
			{
				levelComponentsBuildQueue.Enqueue(levelComponent);
			}
		}

		public void Build(int level)
		{
			this.level = level;
			// TODO: set specific level components parameters by level
			Build();
		}

		public void Build()
		{
			SetLevelComponentsBuildQueue();
		}

		public void Dispose()
		{
			for (int i = levelComponentsArray.Length - 1; i >= 0; i--)
			{
				(levelComponentsArray[i] as IDisposable).Dispose();
			}

			levelComponentsBuildQueue.Clear();
		}

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}