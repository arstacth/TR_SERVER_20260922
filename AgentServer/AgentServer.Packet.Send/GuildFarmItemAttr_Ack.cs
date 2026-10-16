using System.Collections.Generic;
using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildFarmItemAttr_Ack : NetPacket
	{
		public GuildFarmItemAttr_Ack(int GuildNum, List<FarmItemAttr> farmitemattr, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.GetGuildFarmObjectAttr_ACK);
			ns.Write(0);
			ns.Write(GuildNum);
			ns.Write(farmitemattr.Count);
			foreach (FarmItemAttr item in farmitemattr)
			{
				ns.Write(item.AttrType);
				ns.Write(item.AttrValueNumber);
				ns.WriteAnsiFixed_intSize(item.AttrValueString);
				ns.Write((byte)0);
				ns.Write(0L);
			}
			_ = last;
		}
	}
}
