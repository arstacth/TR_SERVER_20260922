using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UseCoupleExpAddItemFail_ACK : NetPacket
	{
		public UseCoupleExpAddItemFail_ACK(int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_USE_COUPLE_EXP_ADD_ITEM_FAILED_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
