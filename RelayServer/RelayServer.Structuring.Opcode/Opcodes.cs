using System;

namespace RelayServer.Structuring.Opcode
{
	[Flags]
	public enum Opcodes
	{
		eRelayServer_REGIST_REQ = 21,
		eRelayServer_REGIST_ACK = 22,
		eRelayServer_P2P_WRAP_REQ = 23,
		eRelayServer_P2P_WRAP_ACK = 24,
		eRelayServer_LIVE_MSG_REQ = 25,
		eRelayServer_LIVE_MSG_ACK = 26,
		eUDPProtocol_PING_REQ = 28,
		eUDPProtocol_PING_ACK = 29,
		eUDPProtocol_FOR_CONNECT_CONFIRM_REQ = 38,
		eUDPProtocol_FOR_CONNECT_CONFIRM_ACK = 39
	}
}
