using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ThankOffering_Rank_ACK : NetPacket
	{
		public ThankOffering_Rank_ACK(List<ThankOfferingRank> ranklist, byte last)
		{
			ns.WriteOP(Opcodes.eServer_THANK_OFFERING_PROTOCOL);
			ns.Write((int)eThankOfferingProtocol.GET_RANK_REQ);
			ns.Write(0);
			ns.Write(ranklist.Count);
			foreach (ThankOfferingRank item in ranklist)
			{
				ns.Write(item.RoomKind);
				ns.Write(item.EXP);
				ns.Write(item.Rank);
				ns.Write(item.Point);
				ns.WriteAnsiFixed_intSize(item.NickName);
				ns.Write(item.isReward);
			}
			_ = last;
		}
	}
}
