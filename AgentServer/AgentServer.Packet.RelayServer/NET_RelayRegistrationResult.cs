using LocalCommons.Network;

namespace AgentServer.Packet.RelayServer.Send
{
	public class NET_RelayRegistrationResult : NetPacket
	{
		public NET_RelayRegistrationResult(bool success)
			: base(0)
		{
			ns.Write((byte)0);
			ns.Write(success);
		}
	}
}
