using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetLobbyQuestEventNotify : NetPacket
	{
		public GetLobbyQuestEventNotify(byte last)
		{
			ns.WriteOP(Opcodes.eServer_LOBBY_QUEST_EVENT_INFO_NOTIFY);
			ns.WriteHex("010400000001000000020000000300000004000000");
		}
	}
}
