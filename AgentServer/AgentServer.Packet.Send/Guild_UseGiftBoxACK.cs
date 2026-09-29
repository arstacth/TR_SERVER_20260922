using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Guild_UseGiftBoxACK : NetPacket
	{
		public Guild_UseGiftBoxACK(int receivedItemNum, int GiftBoxItemNum, int ReaminItemCount, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.USE_GIFT_BOX_ACK);
			ns.Write(receivedItemNum);
			ns.Write(GiftBoxItemNum);
			ns.Write(ReaminItemCount);
			_ = last;
		}
	}
}
