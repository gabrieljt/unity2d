namespace TiledLevel
{
	public interface IMapParams
	{
		int Height { get; set; }
		Tile[,] Tiles { get; set; }
		int Width { get; set; }
	}
}