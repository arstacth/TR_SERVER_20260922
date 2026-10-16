using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_BuyItem : NetPacket
	{
		public GuildPlant_BuyItem(int sellNum, int itemNum, int buyCount, long myContributionPoint, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.BUY_ITEM_ACK);
			ns.Write(sellNum);
			ns.Write(itemNum);
			ns.Write(buyCount);
			ns.Write(0L);
			ns.Write(myContributionPoint);
			_ = last;
		}
	}
}
