using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_StorageReceiveOK : NetPacket
	{
		public Myroom_StorageReceiveOK(int type, List<long> itemList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STORAGE_RECEIVE_ITEM_ACK);
			ns.Write(0);
			ns.Write(type);
			ns.Write(itemList.Count);
			foreach (long item in itemList)
			{
				ns.Write(item);
			}
			_ = last;
		}
	}
}
