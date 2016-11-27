using UnityEngine;

public class MeteorsSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject meteorPrefab;

	[SerializeField]
	private float timeToSpawn = 10f;

	[SerializeField]
	private int quantity = 5;

	private void Start()
	{
		InvokeRandomSpawn();
	}

	private void Spawn()
	{
		var randomQuantity = Random.Range(1, quantity + 1);

		for (int i = 0; i < randomQuantity; i++)
		{
			var outsideViewport = Random.Range(0, 2) % 2 == 0;
			var x = RandomCoordinateOutsideViewport(outsideViewport);
			var y = RandomCoordinateOutsideViewport(!outsideViewport);

			var spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(x, y, -Camera.main.transform.position.z));
			Instantiate(meteorPrefab, spawnPosition, Quaternion.identity, transform);
		}

		InvokeRandomSpawn();
	}

	private void InvokeRandomSpawn()
	{
		var randomTimeToSpawn = Random.Range(1f, timeToSpawn);
		Invoke("Spawn", randomTimeToSpawn);
	}

	private float RandomCoordinateOutsideViewport(bool outsideViewport)
	{
		var randomCoordinate = Random.Range(-0.1f, 1.1f);
		if (outsideViewport && randomCoordinate >= 0f && randomCoordinate <= 1f)
		{
			randomCoordinate += Random.Range(0, 2) % 2 == 0 ? 1f : -1f;
			randomCoordinate = Mathf.Clamp(randomCoordinate, -0.1f, 1.1f);
		}
		return randomCoordinate;
	}
}