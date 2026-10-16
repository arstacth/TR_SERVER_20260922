using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_StorageGiftACK : NetPacket
	{
		public Myroom_StorageGiftACK(Account User, int error, long UniqueNum, string nickname, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STORAGE_GIFT_ITEM_ACK);
			ns.Write(error);
			ns.Write(UniqueNum);
			ns.WriteAnsiFixed_intSize(nickname);
			_ = last;
		}
	}
}
