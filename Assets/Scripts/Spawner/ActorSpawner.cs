using System;
using UnityEngine;

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
            actorSpawner.Perform();
        }
    }
}

#endif

public enum ActorType
{
    Player = 0,
    Exit = 1,
    Slime = 2,
}

public class ActorSpawner : MonoBehaviour
{
    public ActorType type;

    public Vector2 position;

    public Action<ActorSpawner, AActor> Performed = delegate { };

    public bool IsType<TActor>() where TActor : MonoBehaviour
    {
        return ActorsLoader.Actors[(int)type].GetComponent<TActor>();
    }

    public void Perform()
    {
        Debug.Assert((int)type < ActorsLoader.Actors.Length);

        var actor = Instantiate(ActorsLoader.Actors[(int)type], position, Quaternion.identity) as GameObject;
        actor.tag = type.ToString();

        Performed(this, actor.GetComponent<AActor>());
    }

    private void Start()
    {
        Perform();
    }
}