using LocalCommons.Network;
using RelayServer.Structuring.Opcode;

namespace RelayServer.Network.Packet.Send
{
	/// <summary>
	/// eUDPProtocol_PING_ACK = 29. eUDPProtocol is session-first:
	/// uint32 session, uint16 opcode, uint8 flag, uint32 tick.
	/// </summary>
	public sealed class Connect_PingAck : NetPacket
	{
		public Connect_PingAck(int session, int tick)
			: base(2, 0)
		{
			ns.Write(session);
			ns.WriteOP((short)Opcodes.eUDPProtocol_PING_ACK);
			ns.Write((byte)1);
			ns.Write(tick);
		}
	}
}
