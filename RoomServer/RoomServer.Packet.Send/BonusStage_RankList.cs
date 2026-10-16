using System.Linq;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class BonusStage_RankList : NetPacket
	{
		public BonusStage_RankList(NormalRoom room, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_BONUS_STAGE_USER_RANK_LIST_ACK);
			IOrderedEnumerable<Account> orderedEnumerable = from p in room.PlayerList()
				where p.Attribute != 3
				select p into o
				orderby o.BonusStagePoint descending
				select o;
			ns.Write(orderedEnumerable.Count());
			int num = 1;
			foreach (Account item in orderedEnumerable)
			{
				ns.Write(num++);
				ns.WriteAnsiFixed_intSize(item.NickName);
			}
			_ = last;
		}
	}
}
