using UnityEngine;

namespace Actor
{
	public class ActorLoader : MonoBehaviour
	{
		public static ActorLoader Instance { get; private set; }

		[SerializeField]
		private GameObject[] actors;

		[SerializeField]
		public static GameObject[] Actors
		{
			get
			{
				if (Instance == null)
				{
					Instance = FindObjectOfType<ActorLoader>();
				}

				return Instance.actors;
			}
		}

		private void Awake()
		{
			gameObject.isStatic = true;
		}
	}
}