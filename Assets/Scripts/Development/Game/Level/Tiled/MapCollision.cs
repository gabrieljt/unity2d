using System;
using UnityEngine;

namespace Game.Level.Tiled
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapCollision))]
	public class MapCollisionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build"))
			{
				var mapCollision = (MapCollision)target;
				mapCollision.Build();
			}

			if (GUILayout.Button("Dispose"))
			{
				var mapCollision = (MapCollision)target;
				mapCollision.Dispose();
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(Map)
		)]
	public class MapCollision : MonoBehaviour, ILevelComponent, IDestroyable
	{
		[SerializeField]
		private int collidersCount;

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		private Action built = delegate { };

		public Action Built { get { return built; } set { built = value; } }

		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		private void Awake()
		{
			map = GetComponent<Map>();
		}

		public void Build()
		{
			BuildColliders();

			Built();
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
			DestroyColliders();
		}

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}