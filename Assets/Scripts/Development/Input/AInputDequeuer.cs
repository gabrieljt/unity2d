using UnityEngine;

namespace Input
{
	public abstract class AInputDequeuer : MonoBehaviour
	{
		public abstract void OnInputsEnqueued(AInputEnqueuer inputQueue);
	}
}