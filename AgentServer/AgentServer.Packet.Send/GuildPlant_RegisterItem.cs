using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_RegisterItem : NetPacket
	{
		public GuildPlant_RegisterItem(long manageTR, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.REGISTER_ITEM_ACK);
			ns.Write(manageTR);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
