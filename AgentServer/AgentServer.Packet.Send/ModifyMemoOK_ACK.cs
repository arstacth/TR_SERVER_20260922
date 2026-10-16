using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ModifyMemoOK_ACK : NetPacket
	{
		public ModifyMemoOK_ACK(string nickname, string memo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.MODIFY_MEMO_ACK);
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(nickname);
			ns.WriteAnsiFixed_intSize(memo);
			_ = last;
		}
	}
}
