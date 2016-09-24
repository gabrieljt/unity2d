using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Input
{
	public abstract class AInputDequeuer : MonoBehaviour, IDestroyable
	{
		protected HashSet<AInputEnqueuer> inputEnqueuers = new HashSet<AInputEnqueuer>();

		public HashSet<AInputEnqueuer> InputEnqueuers { get { return inputEnqueuers; } }

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