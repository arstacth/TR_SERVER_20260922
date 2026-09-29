using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_GetGuildManageTR : NetPacket
	{
		public GuildPlant_GetGuildManageTR(long tr, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_GUILD_MANAGE_TR_ACK);
			ns.Write(tr);
			_ = last;
		}
	}
}
