using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_GetItemInfoStr : NetPacket
	{
		public Shu_GetItemInfoStr(List<ShuItemInfo> iteminfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.GET_ITEM_INFO_BY_STR_REQ);
			ns.Write(0);
			ns.Write(iteminfos.Count);
			foreach (ShuItemInfo iteminfo in iteminfos)
			{
				ns.Write(iteminfo.itemdescnum);
				ns.Write(iteminfo.itemID);
				ns.Write(iteminfo.gotDateTime);
				ns.Write(iteminfo.count);
				ns.Write(iteminfo.state);
			}
			_ = last;
		}
	}
}
