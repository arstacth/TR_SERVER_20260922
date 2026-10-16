using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetPartyUserList_Ack : NetPacket
	{
		public GetPartyUserList_Ack(Party party, byte last)
		{
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(17L);
			ns.WriteAnsiFixed_intSize(party.LeaderNickName);
			ns.Write(party.Players.Count);
			foreach (Account player in party.Players)
			{
				ns.WriteAnsiFixed_intSize(player.NickName);
				ns.Write(7);
				ns.Write(player.Level);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
