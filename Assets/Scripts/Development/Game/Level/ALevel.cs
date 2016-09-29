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
				var mapDungeonLevel = (ALevelComponent)target;
				mapDungeonLevel.Dispose();
			}
		}
	}

#endif

	public enum LevelState
	{
		Unbuilt,
		Building,
		Built,
		Ready,
		InGame,
		Ended
	}

	// level params
	// build/rebuild (int level)

	public abstract class ALevel : ALevelComponent
	{
		[SerializeField]
		protected LevelState state = LevelState.Unbuilt;

		public abstract void Load(int level);
	}
}