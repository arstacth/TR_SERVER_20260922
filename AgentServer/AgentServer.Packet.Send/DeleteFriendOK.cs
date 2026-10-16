using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DeleteFriendOK : NetPacket
	{
		public DeleteFriendOK(string nickname, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.DELETE_FRIEND_ACK);
			ns.WriteAnsiFixed_intSize(nickname);
			_ = last;
		}
	}
}
