using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FirstLoginMakeStartCharacterFail : NetPacket
	{
		public FirstLoginMakeStartCharacterFail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SELECT_START_CHARACTER_ACK);
			ns.Write(64);
			_ = last;
		}
	}
}
