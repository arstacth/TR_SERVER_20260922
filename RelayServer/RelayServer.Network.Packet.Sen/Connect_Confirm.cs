using LocalCommons.Network;
using RelayServer.Structuring.Opcode;

namespace RelayServer.Network.Packet.Send
{
	/// <summary>
	/// eUDPProtocol FOR_CONNECT_CONFIRM. Client recv of ACK (39) sets
	/// CNetClientInterfaceConfirm +0x48.
	/// eRelayServer ACKs are opcode-first. eUDPProtocol (28+) is session-first:
	/// len|crc|session|opcode|flag. Opcode-first made the client read opcode 39996.
	/// </summary>
	public sealed class Connect_ConfirmReq : NetPacket
	{
		public Connect_ConfirmReq(int session)
			: base(2, 0)
		{
			ns.Write(session);
			ns.WriteOP((short)Opcodes.eUDPProtocol_FOR_CONNECT_CONFIRM_REQ);
			ns.Write((byte)1);
		}
	}

	public sealed class Connect_ConfirmAck : NetPacket
	{
		public Connect_ConfirmAck(int session)
			: base(2, 0)
		{
			ns.Write(session);
			ns.WriteOP((short)Opcodes.eUDPProtocol_FOR_CONNECT_CONFIRM_ACK);
			ns.Write((byte)1);
		}
	}
}
