using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UpdateCoupleInfo2ACK : NetPacket
	{
		public UpdateCoupleInfo2ACK(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_NOTICE_UPDATE_USER_COUPLE_INFO_ACK);
			ns.Write((byte)2);
			ns.Write(User.CoupleInfo.CoupleNum);
			ns.Write(User.CoupleInfo.CoupleType);
			ns.WriteAnsiFixed_intSize(User.CoupleInfo.MateName);
			ns.Write(User.CoupleInfo.CreateTime);
			ns.Write(User.CoupleInfo.MarriedTime);
			ns.Write(User.CoupleInfo.RingChangedTime);
			ns.Write(User.CoupleInfo.CoupleRingNum);
			ns.Write(User.CoupleInfo.MaxRingDays);
			ns.Write(User.CoupleInfo.CoupleLevel);
			ns.Write((short)0);
			ns.Write(User.CoupleInfo.CondDays);
			ns.Write(User.CoupleInfo.AccumulateExp);
			ns.Write(User.CoupleInfo.CoupleRank);
			ns.Write(User.CoupleInfo.CouplePoint);
			ns.Write(0);
			ns.Write(value: false);
			_ = last;
		}
	}
}
