using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_ExploreCheck : NetPacket
	{
		public Shu_ExploreCheck(List<ExploreInfo> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.EXPLORE_CHECK_REQ);
			ns.Write(0);
			ns.Write(infos.Count);
			foreach (ExploreInfo info in infos)
			{
				ns.Write(info.zoneNum);
				ns.Fill(7);
				ns.Write(info.endDateTime);
				ns.Write(info.characterItemID);
			}
			// Thai ACK size 14 = sub+result+count with no trailing last.
			_ = last;
		}
	}
}
