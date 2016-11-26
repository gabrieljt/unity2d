using UnityEngine;

public class TilesetsLoader : MonoBehaviour
{
    [SerializeField]
    private int pixelsPerUnit;

    public static int PixelsPerUnit
    {
        get
        {
            Debug.Assert(FindObjectsOfType<TilesetsLoader>().Length == 1);
            return FindObjectOfType<TilesetsLoader>().pixelsPerUnit;
        }
    }

    [SerializeField]
    private Tileset[] tilesets;

    public static Tileset[] Tilesets
    {
        get
        {
            Debug.Assert(FindObjectsOfType<TilesetsLoader>().Length == 1);
            return FindObjectOfType<TilesetsLoader>().tilesets;
        }
    }

    private void Awake()
    {
        gameObject.isStatic = true;
    }
}