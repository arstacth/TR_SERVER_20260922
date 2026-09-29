using System.Collections.Generic;
using AgentServer.Structuring.Fishing;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CollectFishedItem : NetPacket
	{
		public CollectFishedItem(List<UserFishedItem> fishnetitems, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_FISHING_RECEIVE_FROM_KEEP_NET_ACK);
			ns.Write(0);
			// Challenge ACK is result + two fist bytes. Reward must not look like fists:
			// (0)(0) marker then count + items → onRecv size(%d)(%d)(%d).
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
