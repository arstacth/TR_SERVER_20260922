using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shop_OpenSelectivePackage_ACK : NetPacket
	{
		public Shop_OpenSelectivePackage_ACK(bool result, List<ExchangeItemInfo> infos, int itemid, eShopFailed_REASON failedReason, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_OPEN_SELECTIVE_PACKAGE_ACK);
			if (result)
			{
				ns.Write(0);
				ns.Write((byte)infos.Count);
				foreach (ExchangeItemInfo info in infos)
				{
					ns.Write(info.type);
					ns.Write(info.id);
					ns.Write(info.count);
					ns.Write(int.MaxValue);
				}
			}
			else
			{
				ns.Write((byte)failedReason);
			}
			ns.Write(itemid);
			_ = last;
		}
	}
}
