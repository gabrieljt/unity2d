using System;
using UnityEngine;

namespace Game.Input
{
	public abstract class AInputDequeuer : MonoBehaviour, IDestroyable
	{
		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		public abstract void Dispose();

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}

		public abstract void OnInputsEnqueued(AInputEnqueuer inputQueue);
	}
}