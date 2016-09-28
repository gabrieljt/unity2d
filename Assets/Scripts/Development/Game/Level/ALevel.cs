using UnityEngine;

namespace Game.Level
{
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