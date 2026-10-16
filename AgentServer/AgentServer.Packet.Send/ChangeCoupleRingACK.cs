using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ChangeCoupleRingACK : NetPacket
	{
		public ChangeCoupleRingACK(Account User, bool bOnline, int additionItemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COUPLE_CHANGE_COUPLE_RING_ACK);
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
			ns.Write(additionItemNum);
			ns.Write(bOnline);
			_ = last;
		}
	}
}
