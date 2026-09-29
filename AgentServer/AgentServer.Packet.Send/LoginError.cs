using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginError : NetPacket
	{
		public LoginError(int ErrorCode, byte last, byte suberrcode = 0)
		{
			ns.WriteOP(Opcodes.eServer_LOGIN_FAILED_ACK);
			ns.Write(ErrorCode);
			if (ErrorCode == 12)
			{
				ns.Write(suberrcode);
			}
			_ = last;
		}
	}
}
