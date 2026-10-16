using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_GetGiftList_New : NetPacket
	{
		public Myroom_GetGiftList_New(short startindex, short lastindex, List<UserGiftInfo> GiftList, int TotalItemcount, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_GIFT_ACCEPT_WAIT_LIST_ACK);
			ns.Write(0);
			ns.Write(startindex);
			ns.Write(lastindex);
			ns.Write((short)GiftList.Count);
			ns.Write(TotalItemcount);
			foreach (UserGiftInfo Gift in GiftList)
			{
				ns.Write(Gift.uniNum);
				ns.WriteAnsiFixed_intSize(Gift.sendNickname);
				ns.Write(Gift.itemDescNum);
				ns.Write(Gift.sendDateTime);
				ns.WriteAnsiFixed_intSize(Gift.memo);
				ns.Write(Gift.Expire);
				ns.Write(0);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
