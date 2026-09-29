using System.Collections.Generic;
using System.Linq;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_MiniGame_UpdatePoint : NetPacket
	{
		public GameRoom_MiniGame_UpdatePoint(NormalRoom room, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_MULTIMINIGAME_USER_POINT_NOTIFY);
			ns.Write(room.PlayerCount());
			foreach (var item in from p in room.Players.Values
				where p.Attribute != 3
				join d in room.DropItem on p.UserNum equals d.Key
				select new { p, d } into o
				orderby o.d.Value.MiniGamePoint, o.p.RoomPos
				select o)
			{
				ns.Write(item.p.RoomPos);
				ns.Write(item.d.Value.MiniGamePoint);
			}
			_ = last;
		}
	}
}
