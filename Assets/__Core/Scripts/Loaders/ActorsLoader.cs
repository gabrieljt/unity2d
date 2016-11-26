using UnityEngine;

public class ActorsLoader : MonoBehaviour
{
	[SerializeField]
	private GameObject[] actorsPrefabs;

	[SerializeField]
	public static GameObject[] Actors
	{
		get
		{
			Debug.Assert(FindObjectsOfType<ActorsLoader>().Length == 1);
			return FindObjectOfType<ActorsLoader>().actorsPrefabs;
		}
	}

	private void Awake()
	{
		gameObject.isStatic = true;
	}
}