using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UseCoupleExpAddItem_ACK : NetPacket
	{
		public UseCoupleExpAddItem_ACK(int itemnum, long totalexp, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_USE_COUPLE_EXP_ADD_ITEM_ACK);
			ns.Write(value: true);
			ns.Write(itemnum);
			ns.Write(totalexp);
			_ = last;
		}
	}
}
