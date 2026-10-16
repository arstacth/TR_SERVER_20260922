using LocalCommons.Network;
using NetMsg.Room;

namespace RoomServer.Packet.RoomServer
{
	public sealed class RM_TO_User : NetPacket
	{
		public RM_TO_User(int Session, byte[] packet)
		{
			ns.WriteOP(RMProtocol.RM_TO_User);
			ns.Write(Session);
			ns.Write((ushort)packet.Length);
			ns.Write(packet, 0, packet.Length);
		}
	}
}
