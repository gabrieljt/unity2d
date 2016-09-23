using UnityEngine;
using System;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(ActorSpawner))]
public class ActorSpawnerInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Spawn Actor"))
		{
			var actorSpawner = (ActorSpawner)target;

			actorSpawner.Spawn();

			Debug.Log(actorSpawner.IsType<Character>());
		}
	}
}

#endif

public enum ActorType
{
	Character = 0,
	Exit = 1,
	Slime = 2,
}

[ExecuteInEditMode]
public class ActorSpawner : MonoBehaviour
{
	public ActorType actorType;

	public Vector2 position;

	public Action<ActorSpawner, GameObject> Spawned = delegate { };

	public bool IsType<TActor>() where TActor : MonoBehaviour
	{
		return ActorLoader.Actors[(int)actorType].GetComponent<TActor>();
	}

	public void Spawn()
	{
		Debug.Assert((int)actorType < ActorLoader.Actors.Length);

		var actor = Instantiate(ActorLoader.Actors[(int)actorType], position, Quaternion.identity) as GameObject;
		actor.tag = actorType.ToString();

		Spawned(this, actor);
	}

	private void Start()
	{
		Spawn();
	}
}