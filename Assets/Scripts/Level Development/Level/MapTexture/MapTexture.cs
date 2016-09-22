using System;
using UnityEngine;

namespace Level

{
	[ExecuteInEditMode]
	[RequireComponent(
		typeof(SpriteRenderer)
	)]
	public class MapTexture : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		[SerializeField]
		private MapTilesetType mapTilesetType;

		[SerializeField]
		private Map map;

		public Map Map { get { return map; } }

		public Action<IMapTextureParams> Built = delegate { };

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();

			map = GetComponent<Map>();
			map.Built += OnMapBuilt;
			map.Updated += OnMapUpdated;
		}

		private void OnMapBuilt(IMapParams mapParams)
		{
			IMapTextureParams mapTextureParams = new MapTextureParams();
			Build(mapParams, mapTextureParams);
		}

		private void OnMapUpdated(IMapParams mapParams)
		{
			IMapTextureParams mapTextureParams = new MapTextureParams();
			Build(mapParams, mapTextureParams);
		}

		public void Build(IMapParams mapParams, IMapTextureParams mapTextureRendererParams)
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
			map.Built -= OnMapBuilt;
			map.Updated -= OnMapUpdated;
		}
	}
}