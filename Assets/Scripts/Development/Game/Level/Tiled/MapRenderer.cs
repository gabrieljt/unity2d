using System;
using UnityEngine;

namespace Game.Level.Tiled
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapRenderer))]
	public class MapRendererInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Build"))
			{
				var mapRenderer = (MapRenderer)target;
				mapRenderer.Build();
			}

			if (GUILayout.Button("Dispose"))
			{
				var mapRenderer = (MapRenderer)target;
				mapRenderer.Dispose();
			}
		}
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(SpriteRenderer),
		typeof(Map)
	)]
	public class MapRenderer : MonoBehaviour, IBuildable, IDestroyable, IDisposable
	{
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		[SerializeField]
		private MapTilesetType mapTilesetType;

		[SerializeField]
		private Material spriteMaterial;

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		private Action built = delegate { };

		public Action Built { get { return built; } set { built = value; } }

		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();

			map = GetComponent<Map>();
		}

		public void Build()
		{
			Debug.Assert(mapTilesetType == MapTilesetLoader.MapTilesets[(int)mapTilesetType].Type);
			Debug.Assert((int)mapTilesetType < MapTilesetLoader.MapTilesets.Length);

			var mapTileset = MapTilesetLoader.MapTilesets[(int)mapTilesetType];

			var texture = MapTileset.BuildTexture(map,
				mapTileset.TilesetTexture,
				mapTileset.TilesetTiles);

			spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f, MapTilesetLoader.PixelsPerUnit);
			spriteRenderer.material = spriteMaterial;
		}

		public void Dispose()
		{
			spriteRenderer.sprite = null;
		}

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}