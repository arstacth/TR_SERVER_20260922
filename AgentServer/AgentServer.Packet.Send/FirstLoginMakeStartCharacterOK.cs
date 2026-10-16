using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FirstLoginMakeStartCharacterOK : NetPacket
	{
		public FirstLoginMakeStartCharacterOK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SELECT_START_CHARACTER_ACK);
			ns.Write(0);
			_ = last;
		}
	}
}
