using System;
using UnityEngine;

namespace TiledLevel

{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapCollision))]
	public class MapCollisionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build Colliders"))
			{
				var mapCollision = (MapCollision)target;
				IMapParams mapParams = new MapParams(mapCollision.Map);
				IMapCollisionParams mapCollisionParams = new MapCollisionParams();
				mapCollision.Build(mapParams, mapCollisionParams);
			}

			if (GUILayout.Button("Destroy Colliders"))
			{
				var mapCollision = (MapCollision)target;
				mapCollision.DestroyColliders();
			}
		}
	}

#endif

	[ExecuteInEditMode]
	public class MapCollision : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private int collidersCount;

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		public Action<IMapCollisionParams> Built = delegate { };

		private void Awake()
		{
			map = GetComponent<Map>();
			map.Updated += OnMapUpdated;
		}

		private void OnMapUpdated(IMapParams mapParams)
		{
			IMapCollisionParams mapTextureParams = new MapCollisionParams();
			Build(mapParams, mapTextureParams);
		}

		public void Build(IMapParams mapParams, IMapCollisionParams MapCollisionParams)
		{
			DestroyColliders();
			BuildColliders();

			Built(MapCollisionParams);
		}

		public void DestroyColliders()
		{
			collidersCount = 0;
			var colliders = GetComponents<BoxCollider2D>();

			for (int i = 0; i < colliders.Length; i++)
			{
				DestroyImmediate(colliders[i]);
			}
		}

		private void BuildColliders()
		{
			for (int x = 0; x < map.Width; x++)
			{
				for (int y = 0; y < map.Height; y++)
				{
					if (map.Tiles[x, y].Type == TileType.Wall)
					{
						var collider = gameObject.AddComponent<BoxCollider2D>();
						collider.offset = new Vector2(x, y) + Vector2.one * 0.5f - map.WorldPosition;
						collider.size = Vector2.one;
						++collidersCount;
					}
				}
			}
		}

		public void Dispose()
		{
			map.Updated -= OnMapUpdated;
		}
	}
}