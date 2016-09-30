using UnityEngine;

namespace Game.Level
{
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

		public abstract void Load(ALevelParams level);
	}
}