using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SearchNickNameFail : NetPacket
	{
		public SearchNickNameFail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SEARCH_NICKNAME_ACK);
			ns.Write(73);
			ns.Write((byte)1);
			_ = last;
		}
	}
}
