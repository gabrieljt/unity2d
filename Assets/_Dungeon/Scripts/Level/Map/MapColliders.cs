using UnityEngine;

[RequireComponent(
	typeof(Map)
)]
public class MapColliders : ALevelComponent
{
	[SerializeField]
	private Map map;

	[SerializeField]
	private PhysicsMaterial2D physicsMaterial;

	private void Awake()
	{
		map = GetComponent<Map>();
	}

	public override void Build()
	{
		BuildColliders();

		Built(GetType());
	}

	private void BuildColliders()
	{
		for (int x = 0; x < map.width; x++)
		{
			for (int y = 0; y < map.height; y++)
			{
				if (map.tiles[x, y].Type == TileType.Wall)
				{
					var collider = gameObject.AddComponent<BoxCollider2D>();
					collider.offset = new Vector2(x, y) + Vector2.one * 0.5f - map.Center;
					collider.size = Vector2.one;
					collider.sharedMaterial = physicsMaterial;
					if (!collider.sharedMaterial)
					{
						Debug.LogWarning(name + " physics material not set.");
					}
				}
			}
		}
	}

	public void DestroyColliders()
	{
		var colliders = GetComponents<BoxCollider2D>();

		for (int i = 0; i < colliders.Length; i++)
		{
			Destroy(colliders[i]);
		}
	}

	public override void Dispose()
	{
		DestroyColliders();
	}
}