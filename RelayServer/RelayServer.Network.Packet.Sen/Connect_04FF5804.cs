using LocalCommons.Network;
using RelayServer.Structuring.Opcode;

namespace RelayServer.Network.Packet.Send
{
	public sealed class Connect_04FF5804 : NetPacket
	{
		public Connect_04FF5804(byte[] array)
			: base(2, 0)
		{
			ns.WriteOP(Opcodes.eRelayServer_P2P_WRAP_ACK);
			ns.Write(array, 0);
		}
	}
}
