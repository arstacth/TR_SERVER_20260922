using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginBlackList : NetPacket
	{
		public LoginBlackList(int blockreason, long start, long end, byte last)
		{
			ns.WriteOP(Opcodes.eServer_LOGIN_FAILED_ACK);
			ns.Write(12);
			ns.Write((byte)7);
			ns.Write(blockreason);
			ns.Write(start);
			ns.Write(end);
			_ = last;
		}
	}
}
