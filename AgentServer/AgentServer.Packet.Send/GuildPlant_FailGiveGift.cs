using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_FailGiveGift : NetPacket
	{
		public GuildPlant_FailGiveGift(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GIVE_GIFT_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
