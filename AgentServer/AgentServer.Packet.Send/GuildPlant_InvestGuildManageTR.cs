using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_InvestGuildManageTR : NetPacket
	{
		public GuildPlant_InvestGuildManageTR(long manageTR, long playerTR, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.INVEST_GUILD_MANAGE_TR_ACK);
			ns.Write(manageTR);
			ns.Write(playerTR);
			_ = last;
		}
	}
}
