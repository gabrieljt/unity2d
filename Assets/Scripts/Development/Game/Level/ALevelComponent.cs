using System;
using UnityEngine;

namespace Game.Level
{
	public abstract class ALevelComponent : MonoBehaviour, IBuildable, IDestroyable, IDisposable
	{
		protected Action built = delegate { };

		public Action Built { get { return built; } set { built = value; } }

		protected Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		public abstract void Build();

		public abstract void Dispose();

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}