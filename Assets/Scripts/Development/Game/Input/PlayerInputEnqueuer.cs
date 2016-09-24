using UnityEngine;

namespace Game.Input
{
#if UNITY_EDITOR

	using Game.Actor;
	using UnityEditor;

	[CustomEditor(typeof(PlayerInputEnqueuer))]
	public class PlayerInputEnqueuerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Control selected actor"))
			{
				var playerInputEnqueuer = (PlayerInputEnqueuer)target;
				var playerInputDequeuer = playerInputEnqueuer.SelectedActor.GetComponent<AInputDequeuer>();
				PlayerInputEnqueuer.Add(ref playerInputDequeuer);
			}

			if (GUILayout.Button("Release selected actor"))
			{
				var playerInputEnqueuer = (PlayerInputEnqueuer)target;
				var playerInputDequeuer = playerInputEnqueuer.SelectedActor.GetComponent<AInputDequeuer>();
				PlayerInputEnqueuer.Remove(ref playerInputDequeuer);
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
			if (UnityEngine.Input.anyKey && inputs.Count < maximumInputsPerFrame)
			{
				if (UnityEngine.Input.GetKey(KeyCode.UpArrow))
				{
					inputs.Enqueue(KeyCode.UpArrow);
					return;
				}

				if (UnityEngine.Input.GetKey(KeyCode.DownArrow))
				{
					inputs.Enqueue(KeyCode.DownArrow);
					return;
				}

				if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
				{
					inputs.Enqueue(KeyCode.LeftArrow);
					return;
				}

				if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
				{
					inputs.Enqueue(KeyCode.RightArrow);
					return;
				}
			}
		}

		public static void Add(ref AInputDequeuer inputDequeuer)
		{
			var instance = Instance as AInputEnqueuer;

			instance.Add(ref instance, ref inputDequeuer);

			Debug.LogWarning("PlayerInputEnqueuer set to " + inputDequeuer.name + " | TotalInputDequeuers: " + instance.InputDequeuers.Count);
		}

		public static void Remove(ref AInputDequeuer inputDequeuer)
		{
			var instance = Instance as AInputEnqueuer;

			instance.Remove(ref instance, ref inputDequeuer);

			Debug.LogWarning("PlayerInputEnqueuer releasing from " + inputDequeuer.name + " | TotalInputDequeuers: " + instance.InputDequeuers.Count);
		}

		protected override void OnInputDequeuerDestroyed(MonoBehaviour inputDequeuerBehaviour)
		{
			if (Instance)
			{
				var instance = Instance as AInputEnqueuer;
				var inputDequeuer = inputDequeuerBehaviour.GetComponent<AInputDequeuer>();
				instance.Remove(ref instance, ref inputDequeuer);
			}
		}

		public override void Dispose()
		{
			if (Instance)
			{
				var instance = Instance as AInputEnqueuer;
				foreach (var inputDequeuer in inputDequeuers)
				{
					var inputDequeuerInstance = inputDequeuer;
					instance.Remove(ref instance, ref inputDequeuerInstance);
				}
			}

			inputDequeuers.Clear();
		}
	}
}