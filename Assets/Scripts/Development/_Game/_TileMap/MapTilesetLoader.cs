using UnityEngine;

namespace Game.TileMap
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
		private MapTileset[] tilesets;

		public static MapTileset[] Tilesets
		{
			get
			{
				Debug.Assert(FindObjectsOfType<MapTilesetLoader>().Length == 1);
				return FindObjectOfType<MapTilesetLoader>().tilesets;
			}
		}

		private void Awake()
		{
			gameObject.isStatic = true;
		}
	}
}