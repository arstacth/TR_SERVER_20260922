using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SetGameOption : NetPacket
	{
		public SetGameOption(int option, byte last)
		{
			ns.WriteOP(Opcodes.eServer_USER_INFO_OPTION_SET_ACK);
			ns.Write(0);
			ns.Write(option);
			_ = last;
		}
	}
}
