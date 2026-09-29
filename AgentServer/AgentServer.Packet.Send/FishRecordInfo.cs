using System.Collections.Generic;
using AgentServer.Structuring.Fishing;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FishRecordInfo : NetPacket
	{
		public FishRecordInfo(List<UserFishedItem> fishrecord, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_FISHING_MY_PICTURE_BOOK_ACK);
			ns.Write(0);
			ns.Write(fishrecord.Count);
			foreach (UserFishedItem item in fishrecord)
			{
				ns.Write(item.ItemNum);
				ns.Write(item.Size);
				ns.Write(item.Count);
			}
		}
	}
}
