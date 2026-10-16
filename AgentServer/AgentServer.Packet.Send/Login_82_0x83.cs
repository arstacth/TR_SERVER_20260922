using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Login_82_0x83 : NetPacket
	{
		public Login_82_0x83(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_ANIMAL_AVATAR_ACK);
			ns.Fill(6);
			_ = last;
		}
	}
}
