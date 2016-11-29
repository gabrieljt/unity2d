using System.Collections;
using UnityEngine;

public class SpaceshipLaserGun : MonoBehaviour
{
	[SerializeField]
	private GameObject laserPrefab;

	private bool waitingForUpdate = false;

	public void Fire()
	{
		if (waitingForUpdate)
			return;

		StartCoroutine(InstantiateProjectileCoroutine());
	}

	private IEnumerator InstantiateProjectileCoroutine()
	{
		waitingForUpdate = true;
		yield return 0;

		var laser = Instantiate(laserPrefab, transform.position, Quaternion.identity) as GameObject;
		laser.transform.up = transform.up;

		var laserCollider = laser.GetComponent<Collider2D>();
		var spaceshipColliders = transform.parent.FindChild("Colliders").GetComponentsInChildren<Collider2D>();
		Debug.Assert(spaceshipColliders.Length > 0);
		for (int i = 0; i < spaceshipColliders.Length; i++)
		{
			Physics2D.IgnoreCollision(laserCollider, spaceshipColliders[i]);
		}

		waitingForUpdate = false;
	}
}