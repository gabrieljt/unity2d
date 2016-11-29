using System.Collections;
using UnityEngine;

public class SpaceshipGun : MonoBehaviour
{
	[SerializeField]
	private GameObject projectilePrefab;

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

		var projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity) as GameObject;
		projectile.transform.up = transform.up;

		var projectileCollider = projectile.GetComponent<Collider2D>();
		var ownerColliders = transform.parent.FindChild("Colliders").GetComponentsInChildren<Collider2D>();
		Debug.Assert(ownerColliders.Length > 0);
		for (int i = 0; i < ownerColliders.Length; i++)
		{
			Physics2D.IgnoreCollision(projectileCollider, ownerColliders[i]);
		}

		waitingForUpdate = false;
	}
}