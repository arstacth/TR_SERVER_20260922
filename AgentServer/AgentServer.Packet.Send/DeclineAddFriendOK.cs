using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DeclineAddFriendOK : NetPacket
	{
		public DeclineAddFriendOK(Account User, string nickname, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.DECLINE_FRIEND_ACK);
			ns.WriteAnsiFixed_intSize(nickname);
			_ = last;
		}
	}
}
