using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUnknownResponse7 : NetPacket
	{
		public LoginUnknownResponse7(byte last)
		{
			ns.WriteOP(Opcodes.eServer_KICK_PLAYER_ACK);
			ns.WriteHex("00000000000000573A0000000000000100C0644C2667010000");
			_ = last;
		}
	}
}
