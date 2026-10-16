using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AlchemistDisjoint_ACK : NetPacket
	{
		public AlchemistDisjoint_ACK(int itemDescNum, List<ExchangeItemInfo> exinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALCHEMIST_DISJOINT_ACK);
			ns.Write(0);
			ns.Write(itemDescNum);
			ns.Write(exinfo.Count);
			foreach (ExchangeItemInfo item in exinfo)
			{
				ns.Write(item.type);
				ns.Write(item.id);
				ns.Write(item.count);
				ns.Write(0);
				ns.Write(0);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
