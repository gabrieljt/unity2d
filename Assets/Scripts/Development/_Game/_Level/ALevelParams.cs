using System;
using UnityEngine;

namespace Game.Level
{
	[Serializable]
	public abstract class ALevelParams
	{
		[SerializeField]
		private int level = 1;

		public int Level { get { return level; } }

		public ALevelParams(int level)
		{
			this.level = level;
		}
	}
}