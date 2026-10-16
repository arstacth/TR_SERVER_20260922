using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_AddFavorite : NetPacket
	{
		public Myroom_AddFavorite(int ItemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_ADD_FAVORITES_ACK);
			ns.Write(ItemNum);
			ns.Write(0);
			_ = last;
		}
	}
}
