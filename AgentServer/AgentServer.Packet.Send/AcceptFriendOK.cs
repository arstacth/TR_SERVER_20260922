using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AcceptFriendOK : NetPacket
	{
		public AcceptFriendOK(Account User, string nickname, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.ACCEPT_FRIEND_ACK);
			ns.WriteAnsiFixed_intSize(nickname);
			ns.Write(0);
			ns.Write(0L);
			_ = last;
		}
	}
}
