using System;
using UnityEngine;

namespace Game.Level
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(ALevelComponent))]
	public class ALevelComponentInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			BuildButton();
			DisposeButton();
		}

		protected void BuildButton()
		{
			if (GUILayout.Button("Build"))
			{
				var levelComponent = (ALevelComponent)target;
				levelComponent.Build();
			}
		}

		protected void DisposeButton()
		{
			if (GUILayout.Button("Dispose"))
			{
				var levelComponent = (ALevelComponent)target;
				levelComponent.Dispose();
			}
		}
	}

#endif

	public abstract class ALevelComponent : MonoBehaviour, IBuildable, IDestroyable, IDisposable
	{
		protected Action built = delegate { };

		public Action Built { get { return built; } set { built = value; } }

		protected Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		public abstract void Build();

		public abstract void Dispose();

		public virtual void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}