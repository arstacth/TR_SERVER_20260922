using System;
using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AlchemistHistory_ACK : NetPacket
	{
		public AlchemistHistory_ACK(List<Tuple<int, short, short, short, short>> itemInfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALCHEMIST_HISTORY_ACK);
			ns.Write(0);
			ns.Write(itemInfos.Count);
			foreach (Tuple<int, short, short, short, short> itemInfo in itemInfos)
			{
				ns.Write(itemInfo.Item1);
				ns.Write(itemInfo.Item2);
				ns.Write(itemInfo.Item3);
				ns.Write(itemInfo.Item4);
				ns.Write(itemInfo.Item5);
			}
			// Empty ACK Remain 11/21: packed stops after result+count (no EOF pads).
			_ = last;
		}
	}
}
