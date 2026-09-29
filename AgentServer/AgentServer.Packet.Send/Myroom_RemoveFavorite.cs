using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_RemoveFavorite : NetPacket
	{
		public Myroom_RemoveFavorite(int ItemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_REMOVE_FAVORITES_ACK);
			ns.Write(ItemNum);
			ns.Write(0);
			_ = last;
		}
	}
}
