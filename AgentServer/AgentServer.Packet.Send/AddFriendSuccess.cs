using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AddFriendSuccess : NetPacket
	{
		public AddFriendSuccess(Account User, string nickname, short groupnum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.ADD_FRIEND_ACK);
			ns.WriteAnsiFixed_intSize(nickname);
			ns.Write(0);
			ns.Write(groupnum);
			ns.Write(0L);
			_ = last;
		}
	}
}
