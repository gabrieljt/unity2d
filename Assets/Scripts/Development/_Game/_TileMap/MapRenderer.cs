using Game.Level;
using UnityEngine;

namespace Game.TileMap
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
		private MapTilesetType type;

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
			Debug.Assert(type == MapTilesetLoader.Tilesets[(int)type].Type);
			Debug.Assert((int)type < MapTilesetLoader.Tilesets.Length);

			var tileset = MapTilesetLoader.Tilesets[(int)type];

			var texture = MapTileset.BuildTexture(map,
				tileset.Texture,
				tileset.TilesetTiles);

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