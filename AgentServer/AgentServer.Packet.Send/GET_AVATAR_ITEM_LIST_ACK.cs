using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class GET_AVATAR_ITEM_LIST_ACK : NetPacket
	{
		public GET_AVATAR_ITEM_LIST_ACK(Account User, int startindex, byte remainpage, List<NetItemInfo> iteminfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_AVATAR_ITEM_LIST_ACK);
			ns.Write(startindex);
			ns.Write(iteminfos.Count);
			foreach (NetItemInfo iteminfo in iteminfos)
			{
				if (iteminfo.m_count <= 0 && ShopItemTable.getItemDataFromItemDescNum(iteminfo.m_iItemDescNum, out var itemData) && itemData.m_iType != 1)
				{
					User.activeItem.deleteItem(iteminfo.m_iItemDescNum);
				}
				ns.Write((ushort)(int)iteminfo.m_character);
				ns.Write((ushort)(int)iteminfo.m_position);
				ns.Write((ushort)(int)iteminfo.m_kind);
				ns.Write(iteminfo.m_iItemDescNum);
				ns.Write(iteminfo.m_expireTime);
				ns.Write(iteminfo.m_tGot);
				ns.Write(iteminfo.m_count);
				ns.Write(iteminfo.m_exp);
				ns.Write(iteminfo.m_bHasExpireTime);
				ns.Write(iteminfo.m_bUsing);
			}
			ns.Write(remainpage == 0);
			_ = last;
		}
	}
}
