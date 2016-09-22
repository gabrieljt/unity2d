using UnityEngine;

namespace Level
{
	[ExecuteInEditMode]
	public class MapTilesetLoader : MonoBehaviour
	{
		public static MapTilesetLoader Instance { get; private set; }

		[SerializeField]
		private int pixelsPerUnit;

		public static int PixelsPerUnit { get { return Instance.pixelsPerUnit; } }

		[SerializeField]
		private MapTileset[] mapTilesets;

		public static MapTileset[] MapTilesets { get { return Instance.mapTilesets; } }

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			gameObject.isStatic = true;
		}
	}
}