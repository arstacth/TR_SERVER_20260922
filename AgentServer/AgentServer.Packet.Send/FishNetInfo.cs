using System.Collections.Generic;
using AgentServer.Structuring.Fishing;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FishNetInfo : NetPacket
	{
		public FishNetInfo(List<UserFishedItem> fishnetitems, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_MY_KEEP_NET_ACK);
			ns.Write(0);
			ns.Write(fishnetitems.Count);
			foreach (UserFishedItem fishnetitem in fishnetitems)
			{
				ns.Write(fishnetitem.ItemNum);
				ns.Write(fishnetitem.Size);
				ns.Write(fishnetitem.Count);
			}
			ns.Write(0);
			// No last.
		}
	}
}
