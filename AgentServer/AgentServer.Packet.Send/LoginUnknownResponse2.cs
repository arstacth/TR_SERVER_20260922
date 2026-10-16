using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUnknownResponse2 : NetPacket
	{
		public LoginUnknownResponse2(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_PCROOMUSER_LUCKY_BONUS);
			ns.Write(0);
			_ = last;
		}
	}
}
