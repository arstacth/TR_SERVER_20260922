using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class AG_TO_RM_TO_User : NetPacket
	{
		public AG_TO_RM_TO_User(int Session, int RoomID, byte[] packet)
		{
			ns.WriteOP(RMProtocol.AG_TO_RM_TO_User);
			ns.Write(Session);
			ns.Write(RoomID);
			ns.Write((ushort)packet.Length);
			ns.Write(packet, 0, packet.Length);
		}
	}
}
