using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CancelAddFriendOK : NetPacket
	{
		public CancelAddFriendOK(Account User, string nickname, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.CANCEL_ADD_FRIEND_ACK);
			ns.WriteAnsiFixed_intSize(nickname);
			_ = last;
		}
	}
}
