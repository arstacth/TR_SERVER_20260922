using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_BuyMyRoomSlotOK : NetPacket
	{
		public Myroom_BuyMyRoomSlotOK(int SlotNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_MYROOM_PAY_MYROOMSLOT_RESULT_ACK);
			ns.Write(0);
			ns.Write(SlotNum);
			_ = last;
		}
	}
}
