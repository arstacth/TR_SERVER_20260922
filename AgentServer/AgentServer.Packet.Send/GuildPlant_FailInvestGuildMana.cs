using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_FailInvestGuildManageTR : NetPacket
	{
		public GuildPlant_FailInvestGuildManageTR(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.INVEST_GUILD_MANAGE_TR_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
