using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_ExploreReward : NetPacket
	{
		public Shu_ExploreReward(byte zonenum, long shuitemid, List<ShuRewardInfo> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.EXPLORE_REWARD_ACK);
			ns.Write(0);
			ns.Write(zonenum);
			ns.Write(shuitemid);
			ns.Write(infos.Count);
			foreach (ShuRewardInfo info in infos)
			{
				ns.Write(info.rewardType);
				ns.Write(info.rewardItem);
				ns.Write(info.rewardCount);
				ns.Write(int.MaxValue);
			}
			_ = last;
		}
	}
}
