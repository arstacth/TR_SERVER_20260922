using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TutorialChannel_GiveRewardInfo : NetPacket
	{
		public TutorialChannel_GiveRewardInfo(List<ExchangeItemInfo> exinfo, byte last)
			: this(exinfo, last, error: 0)
		{
		}

		public TutorialChannel_GiveRewardInfo(List<ExchangeItemInfo> exinfo, byte last, int error)
		{
			ns.WriteOP(Opcodes.eServer_TUTORIAL_CHANNEL_GIVE_REWARD_ACK);
			ns.Write(error);
			if (error != 0)
			{
				_ = last;
				return;
			}
			int count = exinfo != null ? exinfo.Count : 0;
			ns.Write(count);
			if (exinfo != null)
			{
				foreach (ExchangeItemInfo item in exinfo)
				{
					ns.Write(item.type);
					ns.Write(item.id);
					ns.Write(item.count);
					ns.Write(int.MaxValue);
					// Client pops 20 bytes per reward (5 ints); 4 ints → Overpop on tutorial 2+.
					ns.Write(0);
				}
			}
			_ = last;
		}
	}
}
