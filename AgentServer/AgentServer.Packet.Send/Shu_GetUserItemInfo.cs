using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_GetUserItemInfo : NetPacket
	{
		public Shu_GetUserItemInfo(int kind, List<ShuItemInfo> iteminfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.GET_USER_ITEM_INFO_REQ);
			ns.Write(0);
			ns.Write(kind);
			ns.Write(iteminfos.Count);
			foreach (ShuItemInfo iteminfo in iteminfos)
			{
				ns.Write(iteminfo.itemdescnum);
				ns.Write(iteminfo.itemID);
				ns.Write(iteminfo.gotDateTime);
				ns.Write(iteminfo.count < 0 ? 0 : iteminfo.count);
				ns.Write(iteminfo.state);
			}
			// Thai ACK size 18 = header+kind+count with no trailing last.
			_ = last;
		}
	}
}
