using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ThankOffering_Reward_ACK : NetPacket
	{
		public ThankOffering_Reward_ACK(int RoomKind, List<ExchangeItemInfo> exinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_THANK_OFFERING_PROTOCOL);
			ns.Write((int)eThankOfferingProtocol.REWARD_REQ);
			ns.Write(0);
			ns.Write(RoomKind);
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
