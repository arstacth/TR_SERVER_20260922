using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;
using TRCommon;

namespace RoomServer.Packet.Send
{
	public sealed class eRoom_UPDATE_ITEM_ONOFF_INFO : NetPacket
	{
		public eRoom_UPDATE_ITEM_ONOFF_INFO(Account User, int itemnum, int OnOffType, int Position, bool bOnOff, NetItemInfo offitem, NetItemInfo onitem, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_UPDATE_ITEM_ONOFF_INFO);
			ns.Write(User.RoomPos);
			ns.Write(itemnum);
			ns.Write(OnOffType);
			ns.Write(Position);
			ns.Write(bOnOff);
			ns.Write(10273232);
			ns.Write(offitem.m_character);
			ns.Write(offitem.m_position);
			ns.Write(offitem.m_kind);
			ns.Write(offitem.m_iItemDescNum);
			ns.Write(offitem.m_expireTime);
			ns.Write(offitem.m_tGot);
			ns.Write(offitem.m_count);
			// No m_exp — two exp ints left RemainSize=8 on packet 572.
			ns.Write(offitem.m_bHasExpireTime);
			ns.Write(offitem.m_bUsing);
			ns.Write(10273232);
			ns.Write(onitem.m_character);
			ns.Write(onitem.m_position);
			ns.Write(onitem.m_kind);
			ns.Write(onitem.m_iItemDescNum);
			ns.Write(onitem.m_expireTime);
			ns.Write(onitem.m_tGot);
			ns.Write(onitem.m_count);
			ns.Write(onitem.m_bHasExpireTime);
			ns.Write(onitem.m_bUsing);
			_ = last;
		}
	}
}
