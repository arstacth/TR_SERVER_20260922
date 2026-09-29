using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetOption : NetPacket
	{
		public GetOption(int Option, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MESSAGE_GET_OPTION_ACK);
			ns.Write(0);
			ns.Write(Option);
			_ = last;
		}
	}
}
