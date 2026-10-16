using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_FFCF0100 : NetPacket
	{
		public Myroom_FFCF0100(bool bTransaction, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_TRANSACTION_ACK);
			ns.Write(bTransaction);
			_ = last;
		}
	}
}
