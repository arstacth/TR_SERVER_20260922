using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetLobbyQuestUserInfo : NetPacket
	{
		public GetLobbyQuestUserInfo(byte last)
		{
			ns.WriteOP(Opcodes.eServer_LOBBY_QUEST_USER_INFO);
			ns.Fill(9);
			_ = last;
		}
	}
}
