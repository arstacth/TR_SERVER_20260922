using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AgentServer.Structuring
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

		public static void AddOrUpdateRoom(int roomsession, NormalRoom room)
		{
			RoomList.AddOrUpdate(roomsession, room, delegate(int k, NormalRoom v)
			{
				v = room;
				return v;
			});
		}

		public static void AddOrUpdatePublicFarmRoom(int roomsession, NormalRoom room)
		{
			PublicFarmRoom.AddOrUpdate(roomsession, room, delegate(int k, NormalRoom v)
			{
				v = room;
				return v;
			});
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

		public static bool CheckRoomServer()
		{
			return RoomServer.RoomServerList.Count > 0;
		}

		public static void RemoveRoom(int roomsession)
		{
			RoomList.TryRemove(roomsession, out var _);
		}

		public static void RemoveRoomFromServer(int rmserverid)
		{
			NormalRoom value;
			foreach (KeyValuePair<int, NormalRoom> item in RoomList.Where((KeyValuePair<int, NormalRoom> a) => a.Value.RoomServerID == rmserverid))
			{
				RoomList.TryRemove(item.Key, out value);
			}
			foreach (KeyValuePair<int, NormalRoom> item2 in PublicFarmRoom.Where((KeyValuePair<int, NormalRoom> w) => w.Value.RoomServerID == rmserverid))
			{
				PublicFarmRoom.TryRemove(item2.Key, out value);
			}
		}
	}
}
