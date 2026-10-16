using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_FailGetInvestorManageTRList : NetPacket
	{
		public GuildPlant_FailGetInvestorManageTRList(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_INVESTOR_MANAGE_TR_LIST_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
