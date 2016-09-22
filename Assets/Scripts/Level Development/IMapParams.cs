namespace Level
{
	public interface IMapParams
	{
		int Height { get; set; }
		Map.Room[] Rooms { get; set; }
		Tile[,] Tiles { get; set; }
		int Width { get; set; }
	}
}