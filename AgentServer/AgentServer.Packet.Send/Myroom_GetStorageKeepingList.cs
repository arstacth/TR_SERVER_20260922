using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_GetStorageKeepingList : NetPacket
	{
		public Myroom_GetStorageKeepingList(List<UserStorageItemInfo> KeepList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STORAGE_ITEM_LIST_ACK);
			ns.Write(0);
			ns.Write(0);
			ns.Write(KeepList.Count);
			foreach (UserStorageItemInfo Keep in KeepList)
			{
				ns.Write(Keep.uniqueNum);
				ns.Write(Keep.itemNum);
				ns.Write(Keep.dateTime);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
