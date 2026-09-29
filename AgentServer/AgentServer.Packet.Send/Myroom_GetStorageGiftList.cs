using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_GetStorageGiftList : NetPacket
	{
		public Myroom_GetStorageGiftList(List<UserStorageItemInfo> GiftList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STORAGE_ITEM_LIST_ACK);
			ns.Write(0);
			ns.Write(1);
			ns.Write(GiftList.Count);
			foreach (UserStorageItemInfo Gift in GiftList)
			{
				ns.Write(Gift.uniqueNum);
				ns.Write(Gift.itemNum);
				ns.Write(Gift.dateTime);
				ns.Write(0);
				ns.WriteAnsiFixed_intSize(Gift.sendNickname);
				ns.WriteAnsiFixed_intSize(Gift.memo);
			}
			_ = last;
		}
	}
}
