using System.Collections.Concurrent;
using System.Linq;

namespace RoomServer.Structuring
{
	public static class Rooms
	{
		public static ConcurrentDictionary<int, NormalRoom> RoomList = new ConcurrentDictionary<int, NormalRoom>();

		public static int GuildMatchRoomID = 1;

		public static ConcurrentDictionary<int, NormalRoom> PublicFarmRoom { get; } = new ConcurrentDictionary<int, NormalRoom>();


		public static void AddRoom(int roomsession, NormalRoom room)
		{
			RoomList.TryAdd(roomsession, room);
		}

		public static bool ExistRoom(int roomsession)
		{
			return RoomList.ContainsKey(roomsession);
		}

		public static NormalRoom GetRoom(int roomsession)
		{
			if (RoomList.TryGetValue(roomsession, out var value))
			{
				return value;
			}
			return null;
		}

		public static NormalRoom GetRoomForGuild(int guildmatchroomid)
		{
			return RoomList.Values.FirstOrDefault((NormalRoom f) => f.GuildMatchRoomID == guildmatchroomid);
		}

		public static void RemoveRoom(int roomsession)
		{
			RoomList.TryRemove(roomsession, out var _);
		}
	}
}
