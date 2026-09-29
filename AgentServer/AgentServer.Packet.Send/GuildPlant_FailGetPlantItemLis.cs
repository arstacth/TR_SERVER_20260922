using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_FailGetPlantItemList : NetPacket
	{
		public GuildPlant_FailGetPlantItemList(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_PLANT_ITEM_LIST_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
