using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUnknownResponse1 : NetPacket
	{
		public LoginUnknownResponse1(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_PCROOMITEM_INFO);
			ns.Fill(5);
			_ = last;
		}
	}
}
