using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetOfficialCompetitionOpenTime_ACK : NetPacket
	{
		public GetOfficialCompetitionOpenTime_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_CHANNEL_INFO_ACK);
			ns.Write(1);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
