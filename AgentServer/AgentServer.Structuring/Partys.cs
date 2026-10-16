using System.Collections.Concurrent;

namespace AgentServer.Structuring
{
	public static class Partys
	{
		public static ConcurrentDictionary<int, Party> PartyList = new ConcurrentDictionary<int, Party>();

		public static void AddParty(int roomsession, Party party)
		{
			PartyList.TryAdd(roomsession, party);
		}

		public static bool ExistParty(int partysession)
		{
			return PartyList.ContainsKey(partysession);
		}

		public static bool GetParty(int partysession, out Party party)
		{
			return PartyList.TryGetValue(partysession, out party);
		}

		public static void RemoveParty(int partysession)
		{
			PartyList.TryRemove(partysession, out var _);
		}
	}
}
