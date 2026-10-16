using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_PassVertification : NetPacket
	{
		public RM_PassVertification(Account User, byte code, byte last)
		{
			ns.WriteOP(RMProtocol.RM_PassVertification_REQ);
			ns.Write(User.Session);
			ns.Write(code);
			ns.Write(last);
		}
	}
}
