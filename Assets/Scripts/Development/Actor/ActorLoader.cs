using UnityEngine;

namespace Actor
{
	public class ActorLoader : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] actors;

		[SerializeField]
		public static GameObject[] Actors
		{
			get
			{
				Debug.Assert(FindObjectsOfType<ActorLoader>().Length == 1);
				return FindObjectOfType<ActorLoader>().actors;
			}
		}

		private void Awake()
		{
			gameObject.isStatic = true;
		}
	}
}