using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TutorialChannel_UserInfo : NetPacket
	{
		public TutorialChannel_UserInfo(List<TutorialChannelInfo> infos, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_TUTORIAL_CHANNEL_USER_INFO_ACK);
			ns.Write(0);
			if (infos == null || infos.Count == 0)
			{
				ns.Write(0);
				return;
			}
			IEnumerable<IGrouping<int, TutorialChannelInfo>> enumerable = from g in infos
				group g by g.Type;
			ns.Write(enumerable.Count());
			foreach (IGrouping<int, TutorialChannelInfo> item in enumerable)
			{
				ns.Write(item.Key);
				ns.Write(item.Count());
				foreach (TutorialChannelInfo item2 in item)
				{
					ns.Write(item2.Level);
				}
			}
		}
	}
}
