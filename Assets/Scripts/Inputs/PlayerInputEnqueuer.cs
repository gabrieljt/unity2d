using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(PlayerInputEnqueuer))]
public class PlayerInputEnqueuerInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Control selected actor"))
		{
			var enqueuer = (PlayerInputEnqueuer)target;
			var dequeuer = enqueuer.SelectedActor.GetComponent<AInputDequeuer>();
			PlayerInputEnqueuer.Add(ref dequeuer);
		}

		if (GUILayout.Button("Release selected actor"))
		{
			var enqueuer = (PlayerInputEnqueuer)target;
			var dequeuer = enqueuer.SelectedActor.GetComponent<AInputDequeuer>();
			PlayerInputEnqueuer.Remove(ref dequeuer);
		}
	}
}

#endif

public class PlayerInputEnqueuer : AInputEnqueuer
{
#if UNITY_EDITOR

	[SerializeField]
	private AActor selectedActor;

	public AActor SelectedActor { get { return selectedActor; } }
#endif

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

	protected override void EnqueueInputs()
	{
		if (Input.anyKey && inputs.Count < maximumInputsPerFrame)
		{
			if (Input.GetKey(KeyCode.UpArrow))
			{
				inputs.Enqueue(KeyCode.UpArrow);
				return;
			}

			if (Input.GetKey(KeyCode.DownArrow))
			{
				inputs.Enqueue(KeyCode.DownArrow);
				return;
			}

			if (Input.GetKey(KeyCode.LeftArrow))
			{
				inputs.Enqueue(KeyCode.LeftArrow);
				return;
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				inputs.Enqueue(KeyCode.RightArrow);
				return;
			}
		}
	}

	public static void Add(ref AInputDequeuer dequeuer)
	{
		var instance = Instance as AInputEnqueuer;

		instance.Add(ref instance, ref dequeuer);
	}

	public static void Remove(ref AInputDequeuer dequeuer)
	{
		var instance = Instance as AInputEnqueuer;

		instance.Remove(ref instance, ref dequeuer);
	}

	protected override void OnDequeuerDestroyed(MonoBehaviour dequeuerBehaviour)
	{
		if (Instance)
		{
			var instance = Instance as AInputEnqueuer;
			var dequeuer = dequeuerBehaviour.GetComponent<AInputDequeuer>();
			instance.Remove(ref instance, ref dequeuer);
		}
	}

	public override void Dispose()
	{
		if (Instance)
		{
			var instance = Instance as AInputEnqueuer;
			foreach (var dequeuer in dequeuers)
			{
				var dequeuerInstance = dequeuer;
				instance.Remove(ref instance, ref dequeuerInstance);
			}
		}

		dequeuers.Clear();
	}
}