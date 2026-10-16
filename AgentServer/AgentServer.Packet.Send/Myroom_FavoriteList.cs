using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_FavoriteList : NetPacket
	{
		public Myroom_FavoriteList(List<int> itemlist, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_LIST_FAVORITES_ACK);
			ns.Write(itemlist.Count);
			foreach (int item in itemlist)
			{
				ns.Write(item);
			}
			ns.Write(0);
			_ = last;
		}
	}
}
