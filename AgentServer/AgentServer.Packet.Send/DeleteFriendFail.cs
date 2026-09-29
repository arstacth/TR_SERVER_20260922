using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DeleteFriendFail : NetPacket
	{
		public DeleteFriendFail(string nickname, short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.DELETE_FRIEND_FAIL_ACK);
			ns.WriteAnsiFixed_intSize(nickname);
			ns.Write(err);
			_ = last;
		}
	}
}
