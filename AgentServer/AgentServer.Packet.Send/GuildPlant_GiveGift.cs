using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_GiveGift : NetPacket
	{
		public GuildPlant_GiveGift(int itemIndexNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GIVE_GIFT_ACK);
			ns.Write(itemIndexNum);
			_ = last;
		}
	}
}
