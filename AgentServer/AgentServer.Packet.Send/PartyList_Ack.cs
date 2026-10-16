using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PartyList_Ack : NetPacket
	{
		public PartyList_Ack(List<Party> SearchPartyList, int maxpage, int page, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(16L);
			ns.Write(maxpage);
			ns.Write(page);
			ns.Write(SearchPartyList.Count);
			foreach (Party SearchParty in SearchPartyList)
			{
				ns.Write(8);
				ns.Write(SearchParty.Players.Count);
				ns.Write(SearchParty.Type);
				ns.Write(SearchParty.LevelLimit);
				ns.Write(SearchParty.LeaderLevel);
				ns.WriteAnsiFixed_intSize(SearchParty.LeaderNickName);
				ns.WriteAnsiFixed_intSize(SearchParty.Title);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
