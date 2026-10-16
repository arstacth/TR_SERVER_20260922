using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_UseLuckyBagFail : NetPacket
	{
		public Myroom_UseLuckyBagFail(int itemnum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_USE_LUCKY_BAG_ACK);
			ns.Write(171);
			ns.Write(itemnum);
			_ = last;
		}
	}
}
