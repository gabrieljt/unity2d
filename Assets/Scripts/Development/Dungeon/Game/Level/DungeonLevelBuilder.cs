using Dungeon.Game.TileMap;
using Game.Level;
using Game.TileMap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Game.Level
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(DungeonLevelBuilder))]
	public class DungeonLevelBuilderInspector : ALevelComponentInspector
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			BuildButton();
			DisposeAllButton();
			DisposeButton();
		}

		private void DisposeAllButton()
		{
			if (GUILayout.Button("Dispose All"))
			{
				var builder = (DungeonLevelBuilder)target;
				builder.DisposeAll();
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(Map),
		typeof(DungeonMap),
		typeof(MapActorSpawners)
	)]
	[RequireComponent(
		typeof(MapColliders),
		typeof(MapRenderer)
	)]
	public class DungeonLevelBuilder : ALevelComponent
	{
		private Queue<ALevelComponent> componentsBuildQueue = new Queue<ALevelComponent>();

		private int componentsBuilt;

		[SerializeField]
		private ALevelComponent[] components = new ALevelComponent[0];

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		[SerializeField]
		private DungeonMap dungeon;

		public DungeonMap Dungeon { get { return dungeon; } }

		[SerializeField]
		private MapActorSpawners spawners;

		public MapActorSpawners Spawners { get { return spawners; } }

		[SerializeField]
		private MapColliders colliders;

		public MapColliders Colliders { get { return colliders; } }

		[SerializeField]
		private MapRenderer renderer;

		public MapRenderer Renderer { get { return renderer; } }

		public override void Build()
		{
			Debug.Assert(componentsBuilt == 0);
			if (componentsBuilt == 0)
			{
				SetComponents();
#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					SetComponentsBuildQueue();
				}
				else
				{
					BuildImmediate();
				}
#else
				SetComponentsBuildQueue();
#endif
			}
		}

		private void SetComponents()
		{
			components = new ALevelComponent[5];
			components[0] = map = GetComponent<Map>();
			components[1] = dungeon = GetComponent<DungeonMap>();
			components[2] = colliders = GetComponent<MapColliders>();
			components[3] = renderer = GetComponent<MapRenderer>();
			components[4] = spawners = GetComponent<MapActorSpawners>();

			foreach (var component in components)
			{
				component.Built += OnComponentBuilt;
			}
		}

		private void OnComponentBuilt(Type type)
		{
			Array.Find(components, component => component.GetType() == type).Built -= OnComponentBuilt;
			++componentsBuilt;

			if (componentsBuilt == components.Length)
			{
				Built(GetType());
			}
		}

		private void SetComponentsBuildQueue()
		{
			foreach (var component in components)
			{
				componentsBuildQueue.Enqueue(component);
			}
		}

		private void Update()
		{
			if (componentsBuildQueue.Count > 0)
			{
				componentsBuildQueue.Dequeue().Build();
			}
		}

		private void BuildImmediate()
		{
			foreach (var component in components)
			{
				component.Build();
			}
		}

		public override void Dispose()
		{
			components = new ALevelComponent[0];
			componentsBuildQueue.Clear();
			componentsBuilt = 0;
		}

		public void DisposeAll()
		{
			Array.Reverse(components);
			foreach (var component in components)
			{
				component.Built -= OnComponentBuilt;
				component.Dispose();
			}

			Dispose();
		}
	}
}