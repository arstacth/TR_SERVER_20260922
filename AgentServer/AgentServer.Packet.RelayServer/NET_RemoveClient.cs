using LocalCommons.Network;

namespace AgentServer.Packet.RelayServer.Send
{
	public class NET_RemoveClient : NetPacket
	{
		public NET_RemoveClient(int session)
			: base(0)
		{
			ns.Write((byte)2);
			ns.Write(session);
		}
	}
}
