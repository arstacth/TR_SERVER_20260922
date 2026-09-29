using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CompetitionEvent_PartyInfo : NetPacket
	{
		public CompetitionEvent_PartyInfo(List<CompetitionPartyInfo> CompetitionPartyInfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMPETITION_EVENT_PARTY_INFO_ACK);
			ns.Write(CompetitionPartyInfo.Count);
			foreach (CompetitionPartyInfo item in CompetitionPartyInfo)
			{
				ns.Write(item.partyType);
				ns.Write(item.point);
				ns.Write(item.userDiffPercent);
			}
			_ = last;
		}
	}
}
