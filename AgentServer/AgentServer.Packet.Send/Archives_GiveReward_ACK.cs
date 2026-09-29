using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Archives_GiveReward_ACK : NetPacket
	{
		public Archives_GiveReward_ACK(List<ExchangeItemInfo> exinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ARCHIVES_USER_REWARD_ACK);
			ns.Write(exinfo.Count);
			foreach (ExchangeItemInfo item in exinfo)
			{
				ns.Write(item.type);
				ns.Write(item.id);
				ns.Write(item.count);
				ns.Write(int.MaxValue);
			}
			_ = last;
		}
	}
}
