using LocalCommons.Network;

namespace RelayServer.Network
{
	public sealed class Send_UDP_Info : NetPacket
	{
		public Send_UDP_Info(int session, short port, string ip)
			: base(0)
		{
			ns.Write((byte)1);
			ns.Write(session);
			ns.Write(port);
			ns.WriteASCIIFixed_intSize(ip);
		}
	}
}
