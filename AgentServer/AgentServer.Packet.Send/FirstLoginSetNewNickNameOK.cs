using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FirstLoginSetNewNickNameOK : NetPacket
	{
		public FirstLoginSetNewNickNameOK(string NickName, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_NICKNAME_ENTER_MY_NICKNAME_ACK);
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(NickName);
			_ = last;
		}
	}
}
