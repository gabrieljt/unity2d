using System;
using UnityEngine;

namespace Game.Level.Tiled

{
	[ExecuteInEditMode]
	[RequireComponent(
		typeof(SpriteRenderer),
		typeof(Map)
	)]
	public class MapRenderer : MonoBehaviour, IDestroyable
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

		public Action Built = delegate { };

		private Action<MonoBehaviour> destroyed = delegate { };

		public Action<MonoBehaviour> Destroyed { get { return destroyed; } set { destroyed = value; } }

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();

			map = GetComponent<Map>();
			map.Updated += OnMapUpdated;
		}

		private void OnMapUpdated()
		{
			Build();
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
			map.Updated -= OnMapUpdated;
		}

		public void OnDestroy()
		{
			Destroyed(this);
			Dispose();
		}
	}
}