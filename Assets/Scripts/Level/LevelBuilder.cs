using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderInspector : ALevelComponentInspector
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
			var builder = (LevelBuilder)target;
			builder.Dispose(true);
		}
	}
}

#endif

[RequireComponent(
	typeof(Map),
	typeof(Dungeon),
	typeof(ActorSpawners)
)]
[RequireComponent(
	typeof(Colliders),
	typeof(Renderer)
)]
public class LevelBuilder : ALevelComponent
{
	private Queue<ALevelComponent> componentsBuildQueue = new Queue<ALevelComponent>();

	private int componentsBuilt;

	[SerializeField]
	private ALevelComponent[] components = new ALevelComponent[0];

	[SerializeField]
	private Map map;

	[SerializeField]
	private Dungeon dungeon;

	[SerializeField]
	private Colliders colliders;

	[SerializeField]
	private Renderer renderer;

	[SerializeField]
	private ActorSpawners actorSpawners;

	public override void Build()
	{
		Debug.Assert(componentsBuilt == 0);
		if (componentsBuilt == 0)
		{
			SetComponents();
			SetComponentsBuildQueue();
		}
	}

	private void SetComponents()
	{
		components = new ALevelComponent[5];
		components[0] = map = GetComponent<Map>();
		components[1] = dungeon = GetComponent<Dungeon>();
		components[2] = colliders = GetComponent<Colliders>();
		components[3] = renderer = GetComponent<Renderer>();
		components[4] = actorSpawners = GetComponent<ActorSpawners>();

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

	public override void Dispose()
	{
		components = new ALevelComponent[0];
		componentsBuildQueue.Clear();
		componentsBuilt = 0;
	}

	public void Dispose(bool disposeDependencies)
	{
		if (disposeDependencies)
		{
			Array.Reverse(components);
			foreach (var component in components)
			{
				component.Built -= OnComponentBuilt;
				component.Dispose();
			}
		}

		Dispose();
	}
}