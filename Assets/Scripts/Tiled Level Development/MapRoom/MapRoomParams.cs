using UnityEngine;

namespace TiledLevel
{
	public class MapRoomParams : IMapRoomParams
	{
		[SerializeField]
		private MapRoom.Room[] rooms;

		public MapRoom.Room[] Rooms
		{
			get
			{
				return rooms;
			}

			set
			{
				rooms = value;
			}
		}

		public MapRoomParams(MapRoom roomMap)
		{
			Rooms = roomMap.Rooms;
		}

		public MapRoomParams()
		{
		}
	}
}