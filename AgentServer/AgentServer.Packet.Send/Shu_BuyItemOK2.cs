using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_BuyItemOK2 : NetPacket
	{
		public Shu_BuyItemOK2(List<ShuItemInfo> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.BUY_ITEM_ACK);
			ns.Write(0);
			ns.Write(infos.Count);
			foreach (ShuItemInfo info in infos)
			{
				ns.Write(info.itemdescnum);
				ns.Write(info.itemID);
				ns.Write(info.gotDateTime);
				ns.Write(info.count);
				ns.Write(info.state);
			}
			_ = last;
		}
	}
}
