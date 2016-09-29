using UnityEngine;

namespace Game.Level
{
	// level params
	// build/rebuild (int level)

	public enum LevelState
	{
		Unloaded,
		Unbuilt,
		Building,
		Built,
	}

	public abstract class ALevel : ALevelComponent
	{
		[SerializeField]
		protected LevelState state = LevelState.Unloaded;

		public abstract void Load(int level);
	}
}