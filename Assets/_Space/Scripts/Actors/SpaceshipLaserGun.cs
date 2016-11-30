using UnityEngine;

public class SpaceshipLaserGun : MonoBehaviour
{
	[SerializeField]
	private GameObject laserPrefab;

	[SerializeField]
	private float fireRate = 1f;

	private bool @lock = false;

	private void Awake()
	{
		Debug.Assert(laserPrefab);
	}

	public void Fire()
	{
		if (@lock)
		{
			return;
		}
		@lock = true;
		InstantiateLaser();
		Invoke("Unlock", fireRate);
	}

	private void InstantiateLaser()
	{
		var laser = Instantiate(laserPrefab, transform.position, Quaternion.identity) as GameObject;
		laser.transform.up = transform.up;

		var laserCollider = laser.GetComponent<Collider2D>();
		var spaceshipColliders = transform.parent.FindChild("Colliders").GetComponentsInChildren<Collider2D>();
		Debug.Assert(spaceshipColliders.Length > 0);
		for (int i = 0; i < spaceshipColliders.Length; i++)
		{
			Physics2D.IgnoreCollision(laserCollider, spaceshipColliders[i]);
		}
	}

	private void Unlock()
	{
		@lock = false;
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		Debug.DrawLine(transform.parent.position, transform.parent.position + transform.up, Color.blue);
	}

#endif
}