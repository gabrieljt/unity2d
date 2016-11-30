using UnityEngine;

public class MeteorsSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject meteorPrefab;

	[SerializeField]
	private float timeToSpawn = 10f;

	[SerializeField]
	private int quantity = 5;

	private void Awake()
	{
		Debug.Assert(meteorPrefab);
	}

	private void Start()
	{
		InvokeRandomSpawn();
	}

	private void InvokeRandomSpawn()
	{
		Invoke("SpawnLoop", Random.Range(1f, timeToSpawn));
	}

	private void SpawnLoop()
	{
		var randomQuantity = Random.Range(1, quantity + 1);

		for (int i = 0; i < randomQuantity; i++)
		{
			Invoke("Spawn", Random.Range(Mathf.Min(quantity, timeToSpawn) / Mathf.Max(quantity, timeToSpawn), Mathf.Max(quantity, timeToSpawn)));
		}

		InvokeRandomSpawn();
	}

	private void Spawn()
	{
		var outsideViewport = Random.Range(0, 2) % 2 == 0;
		var x = RandomCoordinate(outsideViewport);
		var y = RandomCoordinate(!outsideViewport);

		var spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(x, y, -Camera.main.transform.position.z));
		Instantiate(meteorPrefab, spawnPosition, Quaternion.identity, transform);
	}

	private float RandomCoordinate(bool outsideViewport)
	{
		var randomCoordinate = Random.Range(-0.35f, 1.35f);
		if (outsideViewport && randomCoordinate >= 0f && randomCoordinate <= 1f)
		{
			randomCoordinate += Random.Range(0, 2) % 2 == 0 ? 1f : -1f;
			randomCoordinate = Mathf.Clamp(randomCoordinate, -0.35f, 1.35f);
		}
		return randomCoordinate;
	}
}