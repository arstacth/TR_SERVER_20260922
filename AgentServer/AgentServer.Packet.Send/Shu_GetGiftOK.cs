using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_GetGiftOK : NetPacket
	{
		public Shu_GetGiftOK(long shuitemid, int exp, List<ShuRewardInfo> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.GET_GIFT_REQ);
			ns.Write(0);
			ns.Write(shuitemid);
			ns.Write(infos.Count);
			foreach (ShuRewardInfo info in infos)
			{
				ns.Write(info.rewardType);
				ns.Write(info.rewardItem);
				ns.Write(info.rewardCount);
				ns.Write(int.MaxValue);
			}
			ns.Write(exp);
			_ = last;
		}
	}
}
