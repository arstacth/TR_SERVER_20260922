using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Anniversary_InegratedRewardGiveReward_ACK : NetPacket
	{
		public Anniversary_InegratedRewardGiveReward_ACK(List<ExchangeItemInfo> exinfo, int iObjectNum, int actionNum, int count, byte last)
		{
			ns.WriteOP(Opcodes.eServer_INTEGRATED_REWARD_GIVE_REWARD_ACK);
			ns.Write(exinfo.Count);
			foreach (ExchangeItemInfo item in exinfo)
			{
				ns.Write(item.type);
				ns.Write(item.id);
				ns.Write(item.count);
				ns.Write(0);
				ns.Write(0);
				ns.Write(4294967040u);
			}
			ns.Write(iObjectNum);
			ns.Write(actionNum);
			ns.Write(count);
			_ = last;
		}
	}
}
