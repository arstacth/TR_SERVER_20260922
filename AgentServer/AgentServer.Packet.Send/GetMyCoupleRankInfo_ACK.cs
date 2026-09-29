using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetMyCoupleRankInfo_ACK : NetPacket
	{
		public GetMyCoupleRankInfo_ACK(byte type, CoupleRankInfo i, byte last)
			: this(type, 0, i, last)
		{
		}

		public GetMyCoupleRankInfo_ACK(byte type, byte detailRank, CoupleRankInfo i, byte last)
		{
			ns.WriteOP(Opcodes.eServer_RANK_MY_NICKNAME_ACK);
			if (i == null)
			{
				ns.Write(65);
				_ = last;
				return;
			}
			ns.Write(0);
			ns.Write(type);
			ns.Write(i.coupleNum);
			ns.Write(i.point);
			ns.Write(i.rank);
			ns.Write(i.level);
			ns.WriteAnsiFixed_intSize(i.femaleNickName);
			ns.WriteAnsiFixed_intSize(i.maleNickName);
			_ = last;
		}
	}
}
