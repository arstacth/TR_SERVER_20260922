using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_ChangeMyConstributionPointItem : NetPacket
	{
		public GuildPlant_ChangeMyConstributionPointItem(int itemIndexNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.CHANGE_MY_CONSTRIBUTION_POINT_ITEM_ACK);
			ns.Write(itemIndexNum);
			_ = last;
		}
	}
}
