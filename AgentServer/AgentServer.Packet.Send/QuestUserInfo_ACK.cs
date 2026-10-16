using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring;
using AgentServer.Structuring.Mission;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class QuestUserInfo_ACK : NetPacket
	{
		public QuestUserInfo_ACK(bool isUpdate, Account User, int questNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_QUEST_USER_INFO);
			ns.Write(0);
			ns.Write(isUpdate);
			IOrderedEnumerable<KeyValuePair<int, QuestInfo>> orderedEnumerable = User.QuestInfo.OrderBy((KeyValuePair<int, QuestInfo> o) => o.Key);
			if (isUpdate)
			{
				orderedEnumerable = from w in User.QuestInfo
					where w.Key == questNum
					select w into o
					orderby o.Key
					select o;
			}
			ns.Write(orderedEnumerable.Count());
			foreach (KeyValuePair<int, QuestInfo> item in orderedEnumerable)
			{
				ns.Write(item.Value.questNum);
				ns.Write(item.Value.challengeState);
				ns.Write(item.Value.completeCount);
				ns.Write(141359468);
				ns.Write(item.Value.startTime);
				ns.Write(item.Value.endTime);
			}
			_ = last;
		}
	}
}
