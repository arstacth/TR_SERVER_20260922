using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Fishing;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class FishRecordInfo : NetPacket
	{
		public FishRecordInfo(List<UserFishedItem> fishrecord, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_MY_PICTURE_BOOK_ACK);
			ns.Write(0);
			ns.Write(fishrecord.Count);
			foreach (UserFishedItem item in fishrecord)
			{
				ns.Write(item.ItemNum);
				ns.Write(item.Size);
				ns.Write(item.Count);
			}
			_ = last;
		}
	}
}
