using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AddFriendFail : NetPacket
	{
		public AddFriendFail(Account User, string nickname, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.ADD_FRIEND_FAIL_ACK);
			ns.WriteAnsiFixed_intSize(nickname);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
