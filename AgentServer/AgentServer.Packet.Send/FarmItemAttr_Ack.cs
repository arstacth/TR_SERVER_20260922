using System.Collections.Concurrent;
using System.Collections.Generic;
using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FarmItemAttr_Ack : NetPacket
	{
		public FarmItemAttr_Ack(int FarmUniqueNum, ConcurrentDictionary<long, List<FarmItemAttr>> farmitemattr, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.FarmItemAttr_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write((short)1);
			// After buy, a 2-object dump loads farm models and blocks shop close.
			ns.Write(0);
			_ = farmitemattr;
			_ = last;
		}
	}
}
