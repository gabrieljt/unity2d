using System;
using UnityEngine;

namespace Game.Actor
{
	public abstract class AActor : MonoBehaviour, IDestroyable, IDisposable
	{
		public Action<AActor> Enabled = delegate { };

		public Action<AActor> Disabled = delegate { };

		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		public virtual void Enable()
		{
			Enabled(this);
		}

		public virtual void Disable()
		{
			Disabled(this);
		}

		public abstract void Dispose();

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}