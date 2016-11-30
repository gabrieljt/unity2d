using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(PlayerInputEnqueuer))]
public class PlayerInputEnqueuerInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Release Game Object"))
		{
			var enqueuer = (PlayerInputEnqueuer)target;
			var dequeuer = enqueuer.GameObjectDequeueing.GetComponent<AInputDequeuer>();
			PlayerInputEnqueuer.Instance.Remove(PlayerInputEnqueuer.Instance.GameObjectDequeueing);
		}

		if (GUILayout.Button("Control Game Object"))
		{
			var enqueuer = (PlayerInputEnqueuer)target;
			var dequeuer = enqueuer.GameObjectDequeueing.GetComponent<AInputDequeuer>();
			PlayerInputEnqueuer.Instance.Add(PlayerInputEnqueuer.Instance.GameObjectDequeueing);
		}
	}
}

#endif

public class PlayerInputEnqueuer : AInputEnqueuer
{
	public static PlayerInputEnqueuer Instance
	{
		get
		{
			return FindObjectOfType<PlayerInputEnqueuer>();
		}
	}

	[SerializeField]
	private GameObject gameObjectDequeueing;

	public GameObject GameObjectDequeueing { get { return gameObjectDequeueing; } }

	protected override void Awake()
	{
		base.Awake();
		gameObject.isStatic = true;
	}

	public void Add(GameObject gameObject)
	{
		var dequeuer = gameObject.GetComponent<AInputDequeuer>();
		Add(ref dequeuer);
		this.gameObjectDequeueing = gameObject;
	}

	public void Remove(GameObject gameObject)
	{
		var dequeuer = gameObject.GetComponent<AInputDequeuer>();
		Remove(ref dequeuer);
		this.gameObjectDequeueing = null;
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