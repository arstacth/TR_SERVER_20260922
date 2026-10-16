using System.Collections.Generic;
using System.Linq;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TeamStatus : NetPacket
	{
		public TeamStatus(Account User, NormalRoom room, byte last)
		{
			List<Account> list = room.Players.Values.Where((Account w) => w.RoomPos == User.RoomPos || w.RoomPos == User.Partner).ToList();
			ns.WriteOP(RoomOpcodes.eRoom_RABBIT_TURTLE_STATUS_NOTIFY);
			ns.Write(list.Count);
			foreach (Account item in list.OrderBy((Account o) => o.RoomPos))
			{
				ns.Write((int)item.RoomPos);
				ns.Write(item.Animal);
			}
			ns.Write(list.Count);
			foreach (Account item2 in list.OrderBy((Account o) => o.RoomPos))
			{
				ns.Write((int)item2.RoomPos);
				ns.Write(item2.Fatigue);
			}
			ns.Write(User.TeamLeader ? User.RoomPos : User.Partner);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
