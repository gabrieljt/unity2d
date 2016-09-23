using UnityEngine;

namespace TiledLevel
{
	[ExecuteInEditMode]
	public class MapTilesetLoader : MonoBehaviour
	{
		[SerializeField]
		private int pixelsPerUnit;

		public static int PixelsPerUnit
		{
			get
			{
				Debug.Assert(FindObjectsOfType<MapTilesetLoader>().Length == 1);
				return FindObjectOfType<MapTilesetLoader>().pixelsPerUnit;
			}
		}

		[SerializeField]
		private MapTileset[] mapTilesets;

		public static MapTileset[] MapTilesets
		{
			get
			{
				Debug.Assert(FindObjectsOfType<MapTilesetLoader>().Length == 1);
				return FindObjectOfType<MapTilesetLoader>().mapTilesets;
			}
		}

		private void Awake()
		{
			gameObject.isStatic = true;
		}
	}
}