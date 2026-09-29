using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class DisconnectPacket : NetPacket
	{
		public DisconnectPacket(Account User, int msgid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DISCONNECT_FROM_SERVER_ACK);
			ns.Write(msgid);
			_ = last;
		}
	}
}
