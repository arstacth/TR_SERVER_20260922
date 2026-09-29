using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Fishing;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class CollectFishedItem : NetPacket
	{
		public CollectFishedItem(List<UserFishedItem> fishnetitems, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_FISHING_RECEIVE_FROM_KEEP_NET_ACK);
			ns.Write(0);
			// Match Agent: (0)(0) reward marker (challenge uses fists 1-8).
			ns.Write((byte)0);
			ns.Write((byte)0);
			ns.Write(fishnetitems.Count);
			foreach (UserFishedItem fishnetitem in fishnetitems)
			{
				ns.Write(fishnetitem.ItemNum);
				ns.Write(fishnetitem.Size > 0 ? fishnetitem.Size : 1);
				ns.Write(fishnetitem.Count > 0 ? fishnetitem.Count : 1);
			}
		}
	}
}
