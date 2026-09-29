using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CompetitionEvent_PartyJoinOK : NetPacket
	{
		public CompetitionEvent_PartyJoinOK(CompetitionEventHandle.eCompetitionEventResult result, int joinPartyType, int SubPartyType, List<ExchangeItemInfo> exinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMPETITION_EVENT_PARTY_JOIN_ACK);
			ns.Write((int)result);
			if (result == CompetitionEventHandle.eCompetitionEventResult.eCompetitionEventResult_OK)
			{
				// Client stores these as shorts at +0x9788 / +0x978a. Ints made sub-party
				// the reward count, so the lobby never recorded a join and replayed pptimg01.
				ns.Write((short)joinPartyType);
				ns.Write((short)SubPartyType);
				int n = exinfo != null ? exinfo.Count : 0;
				ns.Write(n);
				if (exinfo != null)
				{
					foreach (ExchangeItemInfo item in exinfo)
					{
						ns.Write(item.type);
						ns.Write(item.id);
						ns.Write(item.count);
						ns.Write(int.MaxValue);
						ns.Write(0);
					}
				}
			}
			_ = last;
		}
	}
}
