using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Login_FF7F01_0x180 : NetPacket
	{
		public Login_FF7F01_0x180(byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_CHECK_JUDGEMENT_RESULT_ACK);
			ns.Write((byte)0);
		}
	}
}
