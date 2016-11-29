using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(PlayerInputEnqueuer))]
public class PlayerInputEnqueuerInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Release actor"))
		{
			var enqueuer = (PlayerInputEnqueuer)target;
			var dequeuer = enqueuer.Actor.GetComponent<AInputDequeuer>();
			PlayerInputEnqueuer.Instance.Remove(PlayerInputEnqueuer.Instance.Actor);
		}

		if (GUILayout.Button("Control actor"))
		{
			var enqueuer = (PlayerInputEnqueuer)target;
			var dequeuer = enqueuer.Actor.GetComponent<AInputDequeuer>();
			PlayerInputEnqueuer.Instance.Add(PlayerInputEnqueuer.Instance.Actor);
		}
	}
}

#endif

public class PlayerInputEnqueuer : AInputEnqueuer
{
	[SerializeField]
	private AActor actor;

	public AActor Actor { get { return actor; } }

	public static PlayerInputEnqueuer Instance
	{
		get
		{
			return FindObjectOfType<PlayerInputEnqueuer>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		gameObject.isStatic = true;
	}

	public void Add(AActor actor)
	{
		var dequeuer = actor.GetComponent<AInputDequeuer>();
		Add(ref dequeuer);
		this.actor = actor;
	}

	public void Remove(AActor actor)
	{
		var dequeuer = actor.GetComponent<AInputDequeuer>();
		Remove(ref dequeuer);
		this.actor = null;
	}

	protected override void EnqueueInputs()
	{
		if (Input.anyKey)
		{
			if (Input.GetKey(KeyCode.UpArrow))
			{
				Enqueue(KeyCode.UpArrow);
			}

			if (Input.GetKey(KeyCode.DownArrow))
			{
				Enqueue(KeyCode.DownArrow);
			}

			if (Input.GetKey(KeyCode.LeftArrow))
			{
				Enqueue(KeyCode.LeftArrow);
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				Enqueue(KeyCode.RightArrow);
			}

			if (Input.GetKey(KeyCode.Space))
			{
				Enqueue(KeyCode.Space);
			}
			return;
		}
		Enqueue(KeyCode.None);
		return;
	}
}