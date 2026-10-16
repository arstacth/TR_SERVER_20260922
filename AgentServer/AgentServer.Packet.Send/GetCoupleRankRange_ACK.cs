using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetCoupleRankRange_ACK : NetPacket
	{
		public GetCoupleRankRange_ACK(byte type, List<CoupleRankInfo> ranklist, byte last)
		{
			ns.WriteOP(Opcodes.eServer_RANK_ACK);
			ns.Write(type);
			ns.Write((byte)ranklist.Count);
			foreach (CoupleRankInfo item in ranklist)
			{
				ns.Write(item.coupleNum);
				ns.Write(item.point);
				ns.Write(item.rank);
				ns.Write(item.level);
				ns.WriteAnsiFixed_intSize(item.femaleNickName);
				ns.WriteAnsiFixed_intSize(item.maleNickName);
				ns.Write(0L);
			}
			_ = last;
		}
	}
}
