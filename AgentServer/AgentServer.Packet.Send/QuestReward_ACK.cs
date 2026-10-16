using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class QuestReward_ACK : NetPacket
	{
		public QuestReward_ACK(int questNum, List<ExchangeItemInfo> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_QUEST_REWARD_ACK);
			ns.Write(infos.Count);
			foreach (ExchangeItemInfo info in infos)
			{
				ns.Write(info.type);
				ns.Write(info.id);
				ns.Write(info.count);
				ns.Write(-1);
				ns.Write(-1f);
				ns.Write(1933568512);
			}
			ns.Write(0);
			ns.Write(questNum);
			_ = last;
		}
	}
}
