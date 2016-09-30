using UnityEngine;

namespace Game.Level.Tiled
{
#if UNITY_EDITOR

	using UnityEditor;

	[CustomEditor(typeof(MapRenderer))]
	public class MapRendererInspector : ALevelComponentInspector
	{
	}

#endif

	[ExecuteInEditMode]
	[RequireComponent(
		typeof(SpriteRenderer),
		typeof(Map)
	)]
	public class MapRenderer : ALevelComponent
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

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();

			map = GetComponent<Map>();
		}

		public override void Build()
		{
			Debug.Assert(mapTilesetType == MapTilesetLoader.MapTilesets[(int)mapTilesetType].Type);
			Debug.Assert((int)mapTilesetType < MapTilesetLoader.MapTilesets.Length);

			var mapTileset = MapTilesetLoader.MapTilesets[(int)mapTilesetType];

			var texture = MapTileset.BuildTexture(map,
				mapTileset.TilesetTexture,
				mapTileset.TilesetTiles);

			spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f, MapTilesetLoader.PixelsPerUnit);
			spriteRenderer.material = spriteMaterial;

			Built(GetType());
		}

		public override void Dispose()
		{
			spriteRenderer.sprite = null;
		}
	}
}