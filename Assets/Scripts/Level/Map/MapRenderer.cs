using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(MapRenderer))]
public class MapRendererInspector : ALevelComponentInspector
{
}

#endif

[RequireComponent(
    typeof(SpriteRenderer),
    typeof(Map)
)]
public class MapRenderer : ALevelComponent
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private TilesetType type;

    [SerializeField]
    private Material spriteMaterial;

    [SerializeField]
    private Map map;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        map = GetComponent<Map>();
    }

    public override void Build()
    {
        Debug.Assert(type == TilesetsLoader.Tilesets[(int)type].Type);
        Debug.Assert((int)type < TilesetsLoader.Tilesets.Length);

        var tileset = TilesetsLoader.Tilesets[(int)type];

        var texture = Tileset.BuildTexture(map,
            tileset.Texture,
            tileset.TilesetTiles);

        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f, TilesetsLoader.PixelsPerUnit);
        spriteRenderer.material = spriteMaterial;

        Built(GetType());
    }

    public override void Dispose()
    {
        spriteRenderer.sprite = null;
    }
}