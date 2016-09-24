using System.Collections.Generic;
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

				PlayerInputEnqueuer.Add(playerInputEnqueuer.SelectedActor.GetComponent<AInputDequeuer>());
			}

			if (GUILayout.Button("Release selected actor"))
			{
				var playerInputEnqueuer = (PlayerInputEnqueuer)target;

				PlayerInputEnqueuer.Remove(playerInputEnqueuer.SelectedActor.GetComponent<AInputDequeuer>());
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

		private HashSet<AInputDequeuer> inputDequeuers = new HashSet<AInputDequeuer>();

		public static PlayerInputEnqueuer Instance
		{
			get
			{
				Debug.Assert(FindObjectsOfType<PlayerInputEnqueuer>().Length == 1);
				return FindObjectOfType<PlayerInputEnqueuer>();
			}
		}

		private void Awake()
		{
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

		public static void Add(AInputDequeuer inputDequeuer)
		{
			var instance = RegisterInputDequeuer(inputDequeuer);

			Debug.Assert(!instance.inputDequeuers.Contains(inputDequeuer));
			Debug.Assert(!inputDequeuer.InputEnqueuers.Contains(instance));

			inputDequeuer.InputEnqueuers.Add(instance);
			instance.inputDequeuers.Add(inputDequeuer);
		}

		private static PlayerInputEnqueuer RegisterInputDequeuer(AInputDequeuer inputDequeuer)
		{
			var instance = Instance;

			instance.InputsEnqueued += inputDequeuer.OnInputsEnqueued;
			(inputDequeuer as IDestroyable).Destroyed += OnInputDequeuerDestroyed;

			var otherEnqueuer = inputDequeuer.GetComponent<AInputEnqueuer>();
			if (otherEnqueuer)
			{
				otherEnqueuer.enabled = false;
			}

			return instance;
		}

		public static void Remove(AInputDequeuer inputDequeuer)
		{
			var instance = UnregisterInputDequeuer(inputDequeuer);

			Debug.Assert(instance.inputDequeuers.Contains(inputDequeuer));
			Debug.Assert(inputDequeuer.InputEnqueuers.Contains(instance));

			inputDequeuer.InputEnqueuers.Remove(instance);
			instance.inputDequeuers.Remove(inputDequeuer);
		}

		private static PlayerInputEnqueuer UnregisterInputDequeuer(AInputDequeuer inputDequeuer)
		{
			var instance = Instance;

			instance.InputsEnqueued -= inputDequeuer.OnInputsEnqueued;
			(inputDequeuer as IDestroyable).Destroyed -= OnInputDequeuerDestroyed;

			var otherEnqueuer = inputDequeuer.GetComponent<AInputEnqueuer>();
			if (otherEnqueuer)
			{
				otherEnqueuer.enabled = true;
			}

			return instance;
		}

		private static void OnInputDequeuerDestroyed(MonoBehaviour inputDequeuer)
		{
			Remove(inputDequeuer.GetComponent<AInputDequeuer>());
		}

		public override void Dispose()
		{
			foreach (var inputDequeuer in inputDequeuers)
			{
				UnregisterInputDequeuer(inputDequeuer);
			}

			inputDequeuers.Clear();
		}
	}
}