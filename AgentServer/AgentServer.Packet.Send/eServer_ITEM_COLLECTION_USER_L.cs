using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class eServer_ITEM_COLLECTION_USER_LIST_REQ : NetPacket
	{
		public eServer_ITEM_COLLECTION_USER_LIST_REQ(string UserName, List<int> itemnums, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEM_COLLECTION_USER_LIST_ACK);
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(UserName);
			ns.Write((byte)itemnums.Count);
			foreach (int itemnum in itemnums)
			{
				ns.Write(itemnum);
			}
			_ = last;
		}
	}
}
