using LocalCommons.Network;
using RelayServer.Structuring.Opcode;

namespace RelayServer.Network.Packet.Send
{
	public sealed class Connect_04FF5A04 : NetPacket
	{
		public Connect_04FF5A04()
			: base(2, 0)
		{
			ns.WriteOP(Opcodes.eRelayServer_LIVE_MSG_ACK);
		}
	}
}
