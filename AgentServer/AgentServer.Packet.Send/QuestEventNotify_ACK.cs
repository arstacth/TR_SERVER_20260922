using AgentServer.Holders;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class QuestEventNotify_ACK : NetPacket
	{
		public QuestEventNotify_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_QUEST_EVENT_INFO_NOTIFY);
			ns.Write((byte)1);
			ns.Write(MissionHolder.QuestEventInfo.Count);
			foreach (int item in MissionHolder.QuestEventInfo)
			{
				ns.Write(item);
			}
		}
	}
}
