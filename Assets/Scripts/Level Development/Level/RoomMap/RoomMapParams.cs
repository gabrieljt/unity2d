using UnityEngine;

namespace Level
{
	public class RoomMapParams : IRoomMapParams
	{
		[SerializeField]
		private RoomMap.Room[] rooms;

		public RoomMap.Room[] Rooms
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

		public RoomMapParams(RoomMap roomMap)
		{
			Rooms = roomMap.Rooms;
		}

		public RoomMapParams()
		{
		}
	}
}