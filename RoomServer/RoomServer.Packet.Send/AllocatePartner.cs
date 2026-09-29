using System.Collections.Generic;
using System.Linq;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class AllocatePartner : NetPacket
	{
		public AllocatePartner(NormalRoom room, byte last)
		{
			List<Account> list = room.Players.Values.Where((Account p) => p.Team == 1 && p.Animal == 0).ToList();
			List<Account> list2 = room.Players.Values.Where((Account p) => p.Team == 2 && p.Animal == 0).ToList();
			ns.WriteOP(RoomOpcodes.eRoom_RABBIT_TURTLE_MAKE_TEAM);
			ns.Write(4);
			ns.Write(2);
			ns.Write((int)list[0].RoomPos);
			ns.Write(list[0].Partner);
			ns.Write(2);
			ns.Write((int)list[1].RoomPos);
			ns.Write(list[1].Partner);
			ns.Write(2);
			ns.Write((int)list2[0].RoomPos);
			ns.Write(list2[0].Partner);
			ns.Write(2);
			ns.Write((int)list2[1].RoomPos);
			ns.Write(list2[1].Partner);
			ns.Write(8);
			int i;
			for (i = 0; i <= 7; i++)
			{
				ns.Write(i);
				ns.Write(room.Players.Values.FirstOrDefault((Account p) => p.RoomPos == i).Animal);
			}
			_ = last;
		}
	}
}
