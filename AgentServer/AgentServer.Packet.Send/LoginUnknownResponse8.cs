using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUnknownResponse8 : NetPacket
	{
		public LoginUnknownResponse8(byte last)
		{
			ns.WriteOP(Opcodes.eServer_QUEST_USER_INFO);
			ns.Fill(9);
			_ = last;
		}
	}
}
