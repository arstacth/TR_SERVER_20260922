using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class QuestAdd_ACK : NetPacket
	{
		public QuestAdd_ACK(int questNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_QUEST_ADD_ACK);
			ns.Write(0L);
			ns.Write(questNum);
			_ = last;
		}
	}
}
