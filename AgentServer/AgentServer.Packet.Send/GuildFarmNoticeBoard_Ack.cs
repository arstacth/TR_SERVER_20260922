using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildFarmNoticeBoard_Ack : NetPacket
	{
		public GuildFarmNoticeBoard_Ack(int GuildNum, string Notice, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ModifyGuildFarmNoticeBoardInfo_ACK);
			ns.Write(GuildNum);
			ns.WriteAnsiFixed_intSize(Notice);
			_ = last;
		}
	}
}
