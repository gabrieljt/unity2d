using System;
using UnityEngine;

namespace Actor
{
	public abstract class AActor : MonoBehaviour, IDisposable
	{
		public Action<AActor> Enabled = delegate { };

		public Action<AActor> Disabled = delegate { };

		public Action<AActor> Destroyed = delegate { };

		public virtual void Enable()
		{
			Enabled(this);
		}

		public virtual void Disable()
		{
			Disabled(this);
		}

		public abstract void Dispose();

		private void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}