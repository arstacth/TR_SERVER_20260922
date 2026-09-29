using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_FailChangeMyConstributionPointItem : NetPacket
	{
		public GuildPlant_FailChangeMyConstributionPointItem(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.CHANGE_MY_CONSTRIBUTION_POINT_ITEM_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
