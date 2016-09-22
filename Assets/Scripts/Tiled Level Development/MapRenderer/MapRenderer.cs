using System;
using UnityEngine;

namespace TiledLevel

{
	[ExecuteInEditMode]
	[RequireComponent(
		typeof(SpriteRenderer)
	)]
	public class MapRenderer : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		[SerializeField]
		private MapTilesetType mapTilesetType;

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		public Action<IMapRendererParams> Built = delegate { };

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();

			map = GetComponent<Map>();
			map.Updated += OnMapUpdated;
		}

		private void OnMapUpdated(IMapParams mapParams)
		{
			IMapRendererParams mapRendererParams = new MapRendererParams();
			Build(mapParams, mapRendererParams);
		}

		public void Build(IMapParams mapParams, IMapRendererParams mapTextureRendererParams)
		{
			Debug.Assert(mapTilesetType == MapTilesetLoader.MapTilesets[(int)mapTilesetType].Type);
			Debug.Assert((int)mapTilesetType < MapTilesetLoader.MapTilesets.Length);

			var texture = MapTileset.BuildTexture(mapParams,
				MapTilesetLoader.MapTilesets[(int)mapTilesetType].TilesetTexture,
				MapTilesetLoader.MapTilesets[(int)mapTilesetType].TilesetTiles);

			spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f, MapTilesetLoader.PixelsPerUnit);
		}

		public void Dispose()
		{
			map.Updated -= OnMapUpdated;
		}
	}
}