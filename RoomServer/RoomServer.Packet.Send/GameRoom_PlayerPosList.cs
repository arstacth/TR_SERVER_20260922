using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_PlayerPosList : NetPacket
	{
		public GameRoom_PlayerPosList(List<Account> player, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_USER_ASSIST_ITEMS);
			ns.Write(player.Count);
			foreach (Account item in player)
			{
				ns.Write(item.RoomPos);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
