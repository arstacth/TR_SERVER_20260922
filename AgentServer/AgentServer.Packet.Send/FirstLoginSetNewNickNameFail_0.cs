using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FirstLoginSetNewNickNameFail_0X4C : NetPacket
	{
		public FirstLoginSetNewNickNameFail_0X4C(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_NICKNAME_ENTER_MY_NICKNAME_ACK);
			ns.Write(58);
			_ = last;
		}
	}
	public sealed class FirstLoginSetNewNickNameFail_0x63 : NetPacket
	{
		public FirstLoginSetNewNickNameFail_0x63(string nickname, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CHECK_NICKNAME_CAN_USE_ACK);
			ns.WriteAnsiFixed_intSize(nickname);
			ns.Write((byte)68);
			_ = last;
		}
	}
}
