using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginGetNickName_0X1C : NetPacket
	{
		public LoginGetNickName_0X1C(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_NICKNAME_ACK);
			if (User.noNickName)
			{
				// GET_NICKNAME: result 0x37 opens char-select.
				ns.Write(0x37);
			}
			else
			{
				ns.Write(0);
				ns.WriteAnsiFixed_intSize(User.NickName);
			}
		}
	}
}
