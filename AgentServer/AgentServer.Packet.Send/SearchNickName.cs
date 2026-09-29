using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SearchNickName : NetPacket
	{
		public SearchNickName(string NickName, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SEARCH_NICKNAME_ACK);
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(NickName);
			_ = last;
		}
	}
}
