using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class QuestRemove_ACK : NetPacket
	{
		public QuestRemove_ACK(int questNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_QUEST_REMOVE_ACK);
			ns.Write(0);
			ns.Write(questNum);
			_ = last;
		}
	}
}
