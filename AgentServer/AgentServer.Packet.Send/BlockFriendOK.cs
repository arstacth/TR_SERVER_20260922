using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class BlockFriendOK : NetPacket
	{
		public BlockFriendOK(Account User, string nickname, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.BLOCK_FRIEND_ACK);
			ns.WriteAnsiFixed_intSize(nickname);
			_ = last;
		}
	}
}
